$baseUrl = "http://localhost:5200/api/v1"
$global:report = @()

function Test-ApiEndpoint {
    param(
        [string]$Controller,
        [string]$Action,
        [string]$Method,
        [string]$Url,
        [object]$Body = $null,
        [string]$Token = $null,
        [hashtable]$Headers = @{},
        [bool]$ExpectError = $false
    )

    $requestHeaders = @{ "Content-Type" = "application/json" }
    if ($Token) {
        $requestHeaders["Authorization"] = "Bearer $Token"
    }
    foreach ($key in $Headers.Keys) {
        $requestHeaders[$key] = $Headers[$key]
    }

    $jsonBody = if ($Body) { $Body | ConvertTo-Json -Depth 10 } else { $null }

    try {
        $params = @{
            Uri = $Url
            Method = $Method
            Headers = $requestHeaders
        }
        if ($jsonBody) { $params["Body"] = $jsonBody }

        $response = Invoke-RestMethod @params
        
        $entry = [PSCustomObject]@{
            Controller = $Controller
            Action = $Action
            Method = $Method
            Url = $Url
            Status = "SUCCESS (200/204)"
            SentPayload = if ($jsonBody) { $jsonBody } else { "[None]" }
            ReceivedPayload = ($response | ConvertTo-Json -Depth 5 -Compress)
        }
        $global:report += $entry
        Write-Host " [PASS] $Controller -> $Action ($Method $Url)" -ForegroundColor Green
        return $response
    }
    catch {
        $ex = $_.Exception
        $errResponse = ""
        $statusCode = ""
        if ($_.Exception.Response) {
            $statusCode = [int]$_.Exception.Response.StatusCode
            $stream = $_.Exception.Response.GetResponseStream()
            if ($stream) {
                $reader = New-Object System.IO.StreamReader($stream)
                $errResponse = $reader.ReadToEnd()
            }
        }

        if ($ExpectError) {
            $entry = [PSCustomObject]@{
                Controller = $Controller
                Action = $Action
                Method = $Method
                Url = $Url
                Status = "EXPECTED_VALIDATION_OR_REJECTION ($statusCode)"
                SentPayload = if ($jsonBody) { $jsonBody } else { "[None]" }
                ReceivedPayload = $errResponse
            }
            $global:report += $entry
            Write-Host " [PASS - EXPECTED ERROR] $Controller -> $Action ($Method $Url, Status: $statusCode)" -ForegroundColor Yellow
            return $null
        }

        $entry = [PSCustomObject]@{
            Controller = $Controller
            Action = $Action
            Method = $Method
            Url = $Url
            Status = "ERROR: $($ex.Message) ($statusCode)"
            SentPayload = if ($jsonBody) { $jsonBody } else { "[None]" }
            ReceivedPayload = $errResponse
        }
        $global:report += $entry
        Write-Host " [FAIL] $Controller -> $Action ($Method $Url) Status: $statusCode Message: $($ex.Message)" -ForegroundColor Red
        return $null
    }
}

Write-Host "=== STARTING COMPREHENSIVE ENDPOINT VERIFICATION (ALL 57 ENDPOINTS) ===" -ForegroundColor Cyan

# -------------------------------------------------------------
# 1. AUTH CONTROLLER (11 Endpoints)
# -------------------------------------------------------------
$testEmail = "tester_$(Get-Random)@skillswap.app"
$regRes = Test-ApiEndpoint -Controller "Auth" -Action "Register" -Method "POST" -Url "$baseUrl/Auth/Register" -Body @{
    fullName = "Test Suite User"
    email = $testEmail
    password = "Password@123"
    location = "Austin, TX"
    timeZone = "UTC-6"
}

$loginRes = Test-ApiEndpoint -Controller "Auth" -Action "Login (New User)" -Method "POST" -Url "$baseUrl/Auth/Login" -Body @{
    email = $testEmail
    password = "Password@123"
}
$testerToken = $loginRes.data.accessToken
$testerRefreshToken = $loginRes.data.refreshToken
$testerUserId = $loginRes.data.user.id

# Seeded Logins
$aliceLogin = Test-ApiEndpoint -Controller "Auth" -Action "Login (Alice)" -Method "POST" -Url "$baseUrl/Auth/Login" -Body @{
    email = "alice@skillswap.app"
    password = "Password@123"
}
$aliceToken = $aliceLogin.data.accessToken
$aliceId = $aliceLogin.data.user.id

$bobLogin = Test-ApiEndpoint -Controller "Auth" -Action "Login (Bob)" -Method "POST" -Url "$baseUrl/Auth/Login" -Body @{
    email = "bob@skillswap.app"
    password = "Password@123"
}
$bobToken = $bobLogin.data.accessToken
$bobId = $bobLogin.data.user.id

$adminLogin = Test-ApiEndpoint -Controller "Auth" -Action "Login (Admin)" -Method "POST" -Url "$baseUrl/Auth/Login" -Body @{
    email = "admin@skillswap.app"
    password = "Admin@123456"
}
$adminToken = $adminLogin.data.accessToken

Test-ApiEndpoint -Controller "Auth" -Action "RefreshToken" -Method "POST" -Url "$baseUrl/Auth/RefreshToken" -Body @{
    accessToken = $testerToken
    refreshToken = $testerRefreshToken
}

$twoFaRes = Test-ApiEndpoint -Controller "Auth" -Action "Enable2Fa" -Method "POST" -Url "$baseUrl/Auth/Enable2Fa" -Token $testerToken

Test-ApiEndpoint -Controller "Auth" -Action "Confirm2Fa" -Method "POST" -Url "$baseUrl/Auth/Confirm2Fa" -Body @{ code = "000000" } -Token $testerToken -ExpectError $true

Test-ApiEndpoint -Controller "Auth" -Action "Verify2Fa" -Method "POST" -Url "$baseUrl/Auth/Verify2Fa" -Body @{ twoFactorToken = "dummy_token"; code = "000000" } -ExpectError $true

Test-ApiEndpoint -Controller "Auth" -Action "ForgotPassword" -Method "POST" -Url "$baseUrl/Auth/ForgotPassword" -Body @{
    email = $testEmail
}

Test-ApiEndpoint -Controller "Auth" -Action "ResetPassword" -Method "POST" -Url "$baseUrl/Auth/ResetPassword" -Body @{
    email = $testEmail
    token = "fake-token"
    newPassword = "NewPassword@123"
} -ExpectError $true

Test-ApiEndpoint -Controller "Auth" -Action "GoogleLogin" -Method "POST" -Url "$baseUrl/Auth/GoogleLogin" -Body @{
    idToken = "invalid_token_sample"
} -ExpectError $true

Test-ApiEndpoint -Controller "Auth" -Action "AppleLogin" -Method "POST" -Url "$baseUrl/Auth/AppleLogin" -Body @{
    identityToken = "invalid_token_sample"
} -ExpectError $true

Test-ApiEndpoint -Controller "Auth" -Action "FacebookLogin" -Method "POST" -Url "$baseUrl/Auth/FacebookLogin" -Body @{
    accessToken = "invalid_token_sample"
} -ExpectError $true

# -------------------------------------------------------------
# 2. PROFILE CONTROLLER (4 Endpoints)
# -------------------------------------------------------------
Test-ApiEndpoint -Controller "Profile" -Action "GetProfile" -Method "GET" -Url "$baseUrl/Profile/GetProfile" -Token $aliceToken

Test-ApiEndpoint -Controller "Profile" -Action "UpdateProfile" -Method "PUT" -Url "$baseUrl/Profile/UpdateProfile" -Body @{
    fullName = "Alice Johnson"
    bio = "Senior .NET Architect & Language Enthusiast"
    location = "Cairo, Egypt"
    timeZone = "UTC+2"
} -Token $aliceToken

$availSlots = @(
    @{ dayOfWeek = "Monday"; startTime = "09:00:00"; endTime = "12:00:00"; isRecurring = $true },
    @{ dayOfWeek = "Wednesday"; startTime = "15:00:00"; endTime = "18:00:00"; isRecurring = $true }
)
Test-ApiEndpoint -Controller "Profile" -Action "SetAvailability" -Method "PUT" -Url "$baseUrl/Profile/SetAvailability" -Body $availSlots -Token $aliceToken

Test-ApiEndpoint -Controller "Profile" -Action "GetPublicUserProfile" -Method "GET" -Url "$baseUrl/Profile/GetPublicUserProfile/$bobId"

# -------------------------------------------------------------
# 3. CATEGORIES CONTROLLER (1 Endpoint)
# -------------------------------------------------------------
$cats = Test-ApiEndpoint -Controller "Categories" -Action "GetCategories" -Method "GET" -Url "$baseUrl/Categories/GetCategories"

# -------------------------------------------------------------
# 4. SKILLS CONTROLLER (5 Endpoints)
# -------------------------------------------------------------
$skills = Test-ApiEndpoint -Controller "Skills" -Action "GetSkills" -Method "GET" -Url "$baseUrl/Skills/GetSkills"

$guitarSkill = $skills.data | Where-Object { $_.name -like "*Guitar*" } | Select-Object -First 1
$csharpSkill = $skills.data | Where-Object { $_.name -like "*C#*" } | Select-Object -First 1
$spanishSkill = $skills.data | Where-Object { $_.name -like "*Spanish*" } | Select-Object -First 1

$addedOffered = Test-ApiEndpoint -Controller "Skills" -Action "AddOfferedSkill" -Method "POST" -Url "$baseUrl/Skills/AddOfferedSkill" -Body @{
    skillId = $guitarSkill.id
    proficiencyLevel = "Intermediate"
    yearsOfExperience = 2
    description = "Acoustic chords and fingerpicking."
} -Token $testerToken

if ($addedOffered -and $addedOffered.data) {
    Test-ApiEndpoint -Controller "Skills" -Action "DeleteOfferedSkill" -Method "DELETE" -Url "$baseUrl/Skills/DeleteOfferedSkill/$($addedOffered.data.id)" -Token $testerToken
}

$addedWanted = Test-ApiEndpoint -Controller "Skills" -Action "AddWantedSkill" -Method "POST" -Url "$baseUrl/Skills/AddWantedSkill" -Body @{
    skillId = $spanishSkill.id
    targetLevel = "Beginner"
    priority = "High"
    description = "Want to learn conversational Spanish."
} -Token $testerToken

if ($addedWanted -and $addedWanted.data) {
    Test-ApiEndpoint -Controller "Skills" -Action "DeleteWantedSkill" -Method "DELETE" -Url "$baseUrl/Skills/DeleteWantedSkill/$($addedWanted.data.id)" -Token $testerToken
}

# -------------------------------------------------------------
# 5. DISCOVER CONTROLLER (3 Endpoints)
# -------------------------------------------------------------
Test-ApiEndpoint -Controller "Discover" -Action "GetFeed" -Method "GET" -Url "$baseUrl/Discover/GetFeed" -Token $aliceToken

Test-ApiEndpoint -Controller "Discover" -Action "Search" -Method "GET" -Url "$baseUrl/Discover/Search?keyword=Spanish" -Token $aliceToken

Test-ApiEndpoint -Controller "Discover" -Action "GetRecommendedMatches" -Method "GET" -Url "$baseUrl/Discover/GetRecommendedMatches" -Token $aliceToken

# -------------------------------------------------------------
# 6. SWAP REQUESTS CONTROLLER (6 Endpoints)
# -------------------------------------------------------------
$swapReq = Test-ApiEndpoint -Controller "SwapRequests" -Action "Create" -Method "POST" -Url "$baseUrl/SwapRequests/Create" -Body @{
    receiverId = $bobId
    offeredSkillId = $csharpSkill.id
    requestedSkillId = $spanishSkill.id
    proposedDate = (Get-Date).AddDays(3).ToString("o")
    durationMinutes = 60
    notes = "New swap proposal test."
} -Token $aliceToken

$swapId = $swapReq.data.id

Test-ApiEndpoint -Controller "SwapRequests" -Action "GetIncoming" -Method "GET" -Url "$baseUrl/SwapRequests/GetIncoming" -Token $bobToken

Test-ApiEndpoint -Controller "SwapRequests" -Action "GetOutgoing" -Method "GET" -Url "$baseUrl/SwapRequests/GetOutgoing" -Token $aliceToken

# Counter offer by Bob
Test-ApiEndpoint -Controller "SwapRequests" -Action "CounterOffer" -Method "POST" -Url "$baseUrl/SwapRequests/CounterOffer/$swapId" -Body @{
    newProposedDate = (Get-Date).AddDays(4).ToString("o")
    newDurationMinutes = 60
    counterNotes = "How about 4 days from now instead?"
} -Token $bobToken

# Accept counter offer by Alice (Verified working after fix!)
$acceptedSession = Test-ApiEndpoint -Controller "SwapRequests" -Action "Accept" -Method "POST" -Url "$baseUrl/SwapRequests/Accept/$swapId" -Token $aliceToken
$sessionId = $acceptedSession.data.id

# Create second proposal for Reject test
$swapReq2 = Test-ApiEndpoint -Controller "SwapRequests" -Action "Create (For Reject)" -Method "POST" -Url "$baseUrl/SwapRequests/Create" -Body @{
    receiverId = $bobId
    offeredSkillId = $csharpSkill.id
    requestedSkillId = $spanishSkill.id
    proposedDate = (Get-Date).AddDays(5).ToString("o")
    durationMinutes = 45
    notes = "Proposal to be rejected test."
} -Token $aliceToken

if ($swapReq2 -and $swapReq2.data) {
    Test-ApiEndpoint -Controller "SwapRequests" -Action "Reject" -Method "POST" -Url "$baseUrl/SwapRequests/Reject/$($swapReq2.data.id)" -Body @{
        reason = "Currently fully booked."
    } -Token $bobToken
}

# -------------------------------------------------------------
# 7. SESSIONS CONTROLLER (9 Endpoints)
# -------------------------------------------------------------
Test-ApiEndpoint -Controller "Sessions" -Action "GetSessions" -Method "GET" -Url "$baseUrl/Sessions/GetSessions" -Token $bobToken

Test-ApiEndpoint -Controller "Sessions" -Action "GetById" -Method "GET" -Url "$baseUrl/Sessions/GetById/$sessionId" -Token $bobToken

Test-ApiEndpoint -Controller "Sessions" -Action "Join" -Method "POST" -Url "$baseUrl/Sessions/Join/$sessionId" -Token $aliceToken

Test-ApiEndpoint -Controller "Sessions" -Action "Heartbeat" -Method "POST" -Url "$baseUrl/Sessions/Heartbeat/$sessionId" -Token $aliceToken -Body @{
    inCall = $true
    timestamp = (Get-Date).ToString("o")
}

Test-ApiEndpoint -Controller "Sessions" -Action "CallStatus" -Method "GET" -Url "$baseUrl/Sessions/CallStatus/$sessionId" -Token $bobToken

Test-ApiEndpoint -Controller "Sessions" -Action "IceServers" -Method "GET" -Url "$baseUrl/Sessions/IceServers/$sessionId" -Token $aliceToken

Test-ApiEndpoint -Controller "Sessions" -Action "Leave" -Method "POST" -Url "$baseUrl/Sessions/Leave/$sessionId" -Token $aliceToken

Test-ApiEndpoint -Controller "Sessions" -Action "Complete (Alice)" -Method "POST" -Url "$baseUrl/Sessions/Complete/$sessionId" -Body @{
    notes = "Session went smoothly."
} -Token $aliceToken

Test-ApiEndpoint -Controller "Sessions" -Action "Complete (Bob)" -Method "POST" -Url "$baseUrl/Sessions/Complete/$sessionId" -Body @{
    notes = "Mutual completion confirmed."
} -Token $bobToken

# Cancel test on a separate session
$swapReq3 = Test-ApiEndpoint -Controller "SwapRequests" -Action "Create (For Cancel)" -Method "POST" -Url "$baseUrl/SwapRequests/Create" -Body @{
    receiverId = $bobId
    offeredSkillId = $csharpSkill.id
    requestedSkillId = $spanishSkill.id
    proposedDate = (Get-Date).AddDays(6).ToString("o")
    durationMinutes = 60
    notes = "Proposal for cancel test."
} -Token $aliceToken

if ($swapReq3 -and $swapReq3.data) {
    $cancelSession = Test-ApiEndpoint -Controller "SwapRequests" -Action "Accept (For Cancel)" -Method "POST" -Url "$baseUrl/SwapRequests/Accept/$($swapReq3.data.id)" -Token $bobToken
    if ($cancelSession -and $cancelSession.data) {
        Test-ApiEndpoint -Controller "Sessions" -Action "Cancel" -Method "POST" -Url "$baseUrl/Sessions/Cancel/$($cancelSession.data.id)" -Body @{
            reason = "Scheduling conflict arose."
        } -Token $aliceToken
    }
}

# -------------------------------------------------------------
# 8. CHAT CONTROLLER (3 Endpoints)
# -------------------------------------------------------------
$convs = Test-ApiEndpoint -Controller "Chat" -Action "GetConversations" -Method "GET" -Url "$baseUrl/Chat/GetConversations" -Token $aliceToken
$convId = $convs.data[0].id

Test-ApiEndpoint -Controller "Chat" -Action "SendMessage" -Method "POST" -Url "$baseUrl/Chat/SendMessage/$convId" -Body @{
    content = "Automated test message for endpoint verification."
} -Token $aliceToken

Test-ApiEndpoint -Controller "Chat" -Action "GetMessages" -Method "GET" -Url "$baseUrl/Chat/GetMessages/$convId" -Token $bobToken

# -------------------------------------------------------------
# 9. REVIEWS CONTROLLER (2 Endpoints)
# -------------------------------------------------------------
Test-ApiEndpoint -Controller "Reviews" -Action "CreateReview (Alice reviews Bob)" -Method "POST" -Url "$baseUrl/Reviews/CreateReview" -Body @{
    sessionId = $sessionId
    overallRating = 5
    punctualityScore = 5
    communicationScore = 5
    knowledgeScore = 5
    comment = "Outstanding session and clear feedback!"
} -Token $aliceToken

Test-ApiEndpoint -Controller "Reviews" -Action "GetUserReviews" -Method "GET" -Url "$baseUrl/Reviews/GetUserReviews/$bobId"

# -------------------------------------------------------------
# 10. NOTIFICATIONS CONTROLLER (3 Endpoints)
# -------------------------------------------------------------
$notifs = Test-ApiEndpoint -Controller "Notifications" -Action "GetNotifications" -Method "GET" -Url "$baseUrl/Notifications/GetNotifications" -Token $aliceToken

if ($notifs -and $notifs.data -and $notifs.data.items -and $notifs.data.items.Count -gt 0) {
    $notifId = $notifs.data.items[0].id
    Test-ApiEndpoint -Controller "Notifications" -Action "MarkAsRead" -Method "PUT" -Url "$baseUrl/Notifications/MarkAsRead/$notifId" -Token $aliceToken
}

Test-ApiEndpoint -Controller "Notifications" -Action "MarkAllAsRead" -Method "PUT" -Url "$baseUrl/Notifications/MarkAllAsRead" -Token $aliceToken

# -------------------------------------------------------------
# 11. PAYMENTS CONTROLLER (4 Endpoints)
# -------------------------------------------------------------
Test-ApiEndpoint -Controller "Payments" -Action "CreateDepositCheckout" -Method "POST" -Url "$baseUrl/Payments/CreateDepositCheckout" -Body @{
    amount = 25.00
    successUrl = "http://localhost:5200/success"
    cancelUrl = "http://localhost:5200/cancel"
} -Token $aliceToken

Test-ApiEndpoint -Controller "Payments" -Action "CreateSubscriptionCheckout" -Method "POST" -Url "$baseUrl/Payments/CreateSubscriptionCheckout" -Body @{
    plan = "ProMonthly"
    successUrl = "http://localhost:5200/success"
    cancelUrl = "http://localhost:5200/cancel"
} -Token $aliceToken

Test-ApiEndpoint -Controller "Payments" -Action "GetPaymentHistory" -Method "GET" -Url "$baseUrl/Payments/GetPaymentHistory" -Token $aliceToken

Test-ApiEndpoint -Controller "Payments" -Action "StripeWebhook" -Method "POST" -Url "$baseUrl/Payments/StripeWebhook" -Body @{
    id = "evt_test"
    type = "checkout.session.completed"
}

# -------------------------------------------------------------
# 12. REPORTS CONTROLLER (1 Endpoint)
# -------------------------------------------------------------
Test-ApiEndpoint -Controller "Reports" -Action "CreateReport" -Method "POST" -Url "$baseUrl/Reports/CreateReport" -Body @{
    reportedUserId = $bobId
    reasonCategory = "Spam"
    details = "Automated endpoint verification report test."
} -Token $aliceToken

# -------------------------------------------------------------
# 13. ADMIN CONTROLLER (4 Endpoints)
# -------------------------------------------------------------
Test-ApiEndpoint -Controller "Admin" -Action "GetStats" -Method "GET" -Url "$baseUrl/Admin/GetStats" -Token $adminToken

$adminReports = Test-ApiEndpoint -Controller "Admin" -Action "GetReports" -Method "GET" -Url "$baseUrl/Admin/GetReports" -Token $adminToken

if ($adminReports -and $adminReports.data -and $adminReports.data.Count -gt 0) {
    $reportToResolve = $adminReports.data | Where-Object { $_.status -eq "Pending" } | Select-Object -First 1
    if ($reportToResolve) {
        Test-ApiEndpoint -Controller "Admin" -Action "ResolveReport" -Method "PUT" -Url "$baseUrl/Admin/ResolveReport/$($reportToResolve.id)" -Body @{
            status = "Resolved"
            adminNotes = "Resolved during 57-endpoint verification test."
        } -Token $adminToken
    }
}

Test-ApiEndpoint -Controller "Admin" -Action "UpdateUserStatus" -Method "PUT" -Url "$baseUrl/Admin/UpdateUserStatus/${testerUserId}?isActive=true" -Token $adminToken

# Save Report JSON
$outputPath = Join-Path $PSScriptRoot "FULL_ALL_ENDPOINTS_TEST_RESULTS.json"
$global:report | ConvertTo-Json -Depth 5 | Out-File -FilePath $outputPath -Encoding utf8
Write-Host "=== ENDPOINT VERIFICATION COMPLETED: $($global:report.Count) TEST CASES RUN. RESULTS SAVED TO $outputPath ===" -ForegroundColor Cyan
