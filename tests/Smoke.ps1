param([string]$BaseUrl = 'http://127.0.0.1:5088', [string]$Password = $env:Demo__Password)
Import-Module Microsoft.PowerShell.Utility
$ErrorActionPreference = 'Stop'
if (!$Password) { throw 'Set Demo__Password before testing.' }
if ($BaseUrl -notmatch '^http://127\.0\.0\.1:\d+$') { throw 'Tests only run against a local demo server.' }
$results = [System.Collections.Generic.List[object]]::new()
function Check($name, $ok) {
    $results.Add([pscustomobject]@{ Test=$name; Passed=[bool]$ok })
    Write-Host "$(if($ok){'PASS'}else{'FAIL'}) $name"
}
function GetPage($session, $path) {
    Invoke-WebRequest "$BaseUrl$path" -WebSession $session -SkipHttpErrorCheck
}
function PostForm($session, $path, $body, $tokenPage) {
    $page = GetPage $session $tokenPage
    $match = [regex]::Match($page.Content, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"')
    if (!$match.Success) { throw "Missing CSRF token on $tokenPage ($($page.StatusCode))" }
    $body['__RequestVerificationToken'] = [System.Net.WebUtility]::HtmlDecode($match.Groups[1].Value)
    Invoke-WebRequest "$BaseUrl$path" -WebSession $session -Method Post -Body $body -SkipHttpErrorCheck
}
function Login($employee) {
    $session = [Microsoft.PowerShell.Commands.WebRequestSession]::new()
    if ($employee) {
        $response = PostForm $session '/Auth/EmployeeLogin' @{'EmployeeLogin.EmployeeNo'=$employee; 'EmployeeLogin.Password'=$Password} '/Auth/Login'
        Check "Login $employee" ($response.StatusCode -eq 200 -and $response.Content -notmatch 'Invalid Employee No')
    } else {
        $response = PostForm $session '/Auth/Login' @{'Login.CompanyIdentifier'='demo.hospital';'Login.AdminUserId'='demo.admin';'Login.Password'=$Password} '/Auth/Login'
        Check 'Admin login' ($response.StatusCode -eq 200 -and $response.Content -notmatch 'Invalid credentials')
    }
    return $session
}
$anonymous = [Microsoft.PowerShell.Commands.WebRequestSession]::new()
$r = GetPage $anonymous '/Administrator/Employee'
Check 'Anonymous management request requires login' ($r.Content -match 'name="Login.Password"')
$admin = Login ''
$controllers = 'Company','Employee','Shift','Leave','LeavePolicy','Group','Location','AssignGroup','MarkDay'
foreach($controller in $controllers) {
    foreach($action in 'Index','Create') {
        $r = GetPage $admin "/Administrator/$controller/$action"
        Check "Admin $controller/$action renders" ($r.StatusCode -eq 200)
    }
}
foreach($path in '/Administrator/Dashboard/Lookups','/Administrator/GroupShiftAssign','/Administrator/LeaveCancellation','/SuperUser/Dashboard','/SuperUser/Role','/SuperUser/Role/Create','/SuperUser/AssignRole','/SuperUser/AssignRole/Create','/SuperUser/Module','/SuperUser/Module/Create') {
    $r = GetPage $admin $path; Check "Page $path" ($r.StatusCode -eq 200)
}
$employee = Login 'DEMO-001'
foreach($action in 'ViewCalendar','ChangePassword','ApplyForLeave','LeaveFlexisChart','LeaveHistory','LeavePolicy','InOutTimings','ReadMe') {
    $r=GetPage $employee "/Administrator/GenEmployee/$action"; Check "Employee $action" ($r.StatusCode -eq 200)
}
foreach($path in '/Administrator/Employee','/Administrator/Company','/Administrator/Dashboard/Lookups','/SuperUser/Role','/SuperUser/Module','/Administrator/GenIncharge/TeamMembers') {
    $r=GetPage $employee $path; Check "Employee denied $path" ($r.StatusCode -eq 403)
}
$r=Invoke-WebRequest "$BaseUrl/Administrator/Employee/Delete/1" -WebSession $admin -Method Post -SkipHttpErrorCheck
Check 'Mutation without CSRF rejected' ($r.StatusCode -eq 400)
$supervisor=Login 'DEMO-SUP'
foreach($action in 'ViewCalendar','TeamMembers','EmployeeView','TeamLeaveRequests','LeaveFlexisChart','ShiftScheduling','InOutTimings','LeaveSheet','UpdateCalendar') {
    $r=GetPage $supervisor "/Administrator/GenIncharge/$action"; Check "Supervisor $action" ($r.StatusCode -eq 200)
}
$r=PostForm $employee '/Administrator/GenEmployee/ApplyForLeave' @{SelectedLeaveType='Invented';FromDate=(Get-Date).AddDays(30).ToString('yyyy-MM-dd');ToDate=(Get-Date).AddDays(30).ToString('yyyy-MM-dd');Reason='QA invalid leave'} '/Administrator/GenEmployee/ApplyForLeave'
Check 'Unassigned leave type rejected' ($r.Content -match 'Select a leave type assigned')
$r=GetPage $employee '/Administrator/GenEmployee/ViewCalendar?month=13&year=2026'
Check 'Invalid calendar month does not crash' ($r.StatusCode -in 200,400)
$bad=[Microsoft.PowerShell.Commands.WebRequestSession]::new()
$r=PostForm $bad '/Auth/EmployeeLogin' @{'EmployeeLogin.EmployeeNo'='DEMO-OFF';'EmployeeLogin.Password'=$Password} '/Auth/Login'
Check 'Inactive employee cannot login' ($r.Content -match 'Invalid Employee No')
. "$PSScriptRoot/Workflows.ps1"
New-Item -ItemType Directory -Force "$PSScriptRoot/results" | Out-Null
$results | ConvertTo-Json | Set-Content "$PSScriptRoot/results/smoke.json"
$failed=@($results | Where-Object { !$_.Passed })
Write-Host "$($results.Count - $failed.Count)/$($results.Count) passed"
if($failed.Count) { exit 1 }


