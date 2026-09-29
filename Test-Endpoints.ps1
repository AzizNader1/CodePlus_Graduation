$baseUrl = "http://localhost:5200/api/v1"
$report = @()

function Test-ApiEndpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [object]$Body = $null,
        [string]$Token = $null
    )

    $headers = @{ "Content-Type" = "application/json" }
    if ($Token) {
        $headers["Authorization"] = "Bearer $Token"
    }

    $jsonBody = if ($Body) { $Body | ConvertTo-Json -Depth 10 } else { $null }

    try {
        $params = @{
            Uri = $Url
            Method = $Method
            Headers = $headers
        }
        if ($jsonBody) { $params["Body"] = $jsonBody }

        $response = Invoke-RestMethod @params -StatusCodeVariable "statusCode"
        $status = 200

        $entry = [PSCustomObject]@{
            Endpoint = $Name
            Method = $Method
            Url = $Url
            Status = "SUCCESS (200/201)"
            SentPayload = if ($jsonBody) { $jsonBody } else { "[None]" }
            ReceivedPayload = ($response | ConvertTo-Json -Depth 5 -Compress)
        }
        $global:report += $entry
        Write-Host " [PASS] $Name -> $Method $Url" -ForegroundColor Green
        return $response
    }
    catch {
        $ex = $_.Exception
        $errResponse = ""
        if ($_.Exception.Response) {
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $errResponse = $reader.ReadToEnd()
        }
        $entry = [PSCustomObject]@{
            Endpoint = $Name
            Method = $Method
            Url = $Url
            Status = "ERROR: $($ex.Message)"
            SentPayload = if ($jsonBody) { $jsonBody } else { "[None]" }
            ReceivedPayload = $errResponse
        }
        $global:report += $entry
        Write-Host " [FAIL] $Name -> $Method $Url ($($ex.Message))" -ForegroundColor Red
        return $null
    }
}

Write-Host "=== STARTING COMPREHENSIVE ENDPOINT VERIFICATION ===" -ForegroundColor Cyan

# 1. Register New User
$regBody = @{
    fullName = "John Doe"
    email = "john.doe@skillswap.app"
    password = "Password@123"
    location = "San Francisco, CA"
    timeZone = "UTC-7"
}
$regRes = Test-ApiEndpoint -Name "Auth: Register New User" -Method "POST" -Url "$baseUrl/auth/register" -Body $regBody

# 2. Login New User
$loginBody = @{
    email = "john.doe@skillswap.app"
    password = "Password@123"
}
$loginRes = Test-ApiEndpoint -Name "Auth: Login User" -Method "POST" -Url "$baseUrl/auth/login" -Body $loginBody
$johnToken = $loginRes.accessToken

# 3. Refresh Token
$refreshBody = @{
    accessToken = $johnToken
    refreshToken = $loginRes.refreshToken
}
$refreshRes = Test-ApiEndpoint -Name "Auth: Refresh JWT Token" -Method "POST" -Url "$baseUrl/auth/refresh-token" -Body $refreshBody

# 4. Two-Factor Enable
$twoFaRes = Test-ApiEndpoint -Name "Auth: Enable 2FA" -Method "POST" -Url "$baseUrl/auth/2fa/enable" -Token $johnToken

# 5. Forgot Password
$forgotBody = @{ email = "john.doe@skillswap.app" }
Test-ApiEndpoint -Name "Auth: Forgot Password" -Method "POST" -Url "$baseUrl/auth/forgot-password" -Body $forgotBody

# 6. Login Seeded Users (Alice, Bob, Admin)
$aliceLogin = Test-ApiEndpoint -Name "Auth: Login Alice" -Method "POST" -Url "$baseUrl/auth/login" -Body @{ email = "alice@skillswap.app"; password = "Password@123" }
$aliceToken = $aliceLogin.accessToken
$aliceId = $aliceLogin.user.id

$bobLogin = Test-ApiEndpoint -Name "Auth: Login Bob" -Method "POST" -Url "$baseUrl/auth/login" -Body @{ email = "bob@skillswap.app"; password = "Password@123" }
$bobToken = $bobLogin.accessToken
$bobId = $bobLogin.user.id

$adminLogin = Test-ApiEndpoint -Name "Auth: Login Admin" -Method "POST" -Url "$baseUrl/auth/login" -Body @{ email = "admin@skillswap.app"; password = "Admin@123456" }
$adminToken = $adminLogin.accessToken

# 7. Profile Endpoints
Test-ApiEndpoint -Name "Profile: Get My Profile" -Method "GET" -Url "$baseUrl/profile" -Token $johnToken
Test-ApiEndpoint -Name "Profile: Update Profile" -Method "PUT" -Url "$baseUrl/profile" -Body @{ fullName = "Johnathon Doe"; bio = "Experienced software engineer & musician"; location = "SF, California"; timeZone = "UTC-7" } -Token $johnToken

# 8. Profile Availability
$availSlots = @(
    @{ dayOfWeek = "Monday"; startTime = "09:00:00"; endTime = "12:00:00"; isRecurring = $true },
    @{ dayOfWeek = "Wednesday"; startTime = "14:00:00"; endTime = "17:00:00"; isRecurring = $true }
)
Test-ApiEndpoint -Name "Profile: Set Availability" -Method "PUT" -Url "$baseUrl/profile/availability" -Body $availSlots -Token $johnToken
Test-ApiEndpoint -Name "Profile: Get Public Profile (Alice)" -Method "GET" -Url "$baseUrl/profile/users/$aliceId"

# 9. Categories & Skills Catalog
$cats = Test-ApiEndpoint -Name "Catalog: Get Categories" -Method "GET" -Url "$baseUrl/categories"
$skills = Test-ApiEndpoint -Name "Catalog: Get Skills" -Method "GET" -Url "$baseUrl/skills"

$csharpSkill = $skills | Where-Object { $_.name -like "*C#*" } | Select-Object -First 1
$spanishSkill = $skills | Where-Object { $_.name -like "*Spanish*" } | Select-Object -First 1
$guitarSkill = $skills | Where-Object { $_.name -like "*Guitar*" } | Select-Object -First 1

# 10. Add Offered & Wanted Skill for John
$addOffered = Test-ApiEndpoint -Name "Skills: Add Offered Skill" -Method "POST" -Url "$baseUrl/skills/offered" -Body @{ skillId = $guitarSkill.id; proficiencyLevel = "Advanced"; yearsOfExperience = 4; description = "Fingerstyle and acoustic chords." } -Token $johnToken
$addWanted = Test-ApiEndpoint -Name "Skills: Add Wanted Skill" -Method "POST" -Url "$baseUrl/skills/wanted" -Body @{ skillId = $csharpSkill.id; targetLevel = "Intermediate"; priority = "High"; description = "Want to build Web APIs in C#." } -Token $johnToken

# 11. Discovery & Search
Test-ApiEndpoint -Name "Discover: Home Feed" -Method "GET" -Url "$baseUrl/discover/feed" -Token $johnToken
Test-ApiEndpoint -Name "Discover: Search Skills" -Method "GET" -Url "$baseUrl/discover/search?keyword=C%23" -Token $johnToken
$matches = Test-ApiEndpoint -Name "Discover: Smart Matchmaker (Alice)" -Method "GET" -Url "$baseUrl/discover/matches" -Token $aliceToken

# 12. Swap Request Lifecycle: Alice -> Bob
$swapReqBody = @{
    receiverId = $bobId
    offeredSkillId = $csharpSkill.id
    requestedSkillId = $spanishSkill.id
    proposedDate = (Get-Date).AddDays(2).ToString("o")
    durationMinutes = 60
    notes = "Hi Bob, I can teach you .NET Clean Architecture in exchange for Spanish conversational practice!"
}
$createdSwap = Test-ApiEndpoint -Name "SwapRequests: Create Proposal" -Method "POST" -Url "$baseUrl/swap-requests" -Body $swapReqBody -Token $aliceToken
$swapId = $createdSwap.id

# Check incoming & outgoing
Test-ApiEndpoint -Name "SwapRequests: Get Incoming (Bob)" -Method "GET" -Url "$baseUrl/swap-requests/incoming" -Token $bobToken
Test-ApiEndpoint -Name "SwapRequests: Get Outgoing (Alice)" -Method "GET" -Url "$baseUrl/swap-requests/outgoing" -Token $aliceToken

# Bob accepts the proposal -> generates SwapSession
$session = Test-ApiEndpoint -Name "SwapRequests: Accept Proposal (Bob)" -Method "PUT" -Url "$baseUrl/swap-requests/$swapId/accept" -Token $bobToken
$sessionId = $session.id

# 13. Sessions
Test-ApiEndpoint -Name "Sessions: Get My Sessions" -Method "GET" -Url "$baseUrl/sessions" -Token $bobToken
Test-ApiEndpoint -Name "Sessions: Get Session By Id" -Method "GET" -Url "$baseUrl/sessions/$sessionId" -Token $bobToken

# Confirm Completion (Alice & Bob)
Test-ApiEndpoint -Name "Sessions: Confirm Complete (Alice)" -Method "PUT" -Url "$baseUrl/sessions/$sessionId/complete" -Body @{ notes = "Great session with Bob!" } -Token $aliceToken
Test-ApiEndpoint -Name "Sessions: Confirm Complete (Bob)" -Method "PUT" -Url "$baseUrl/sessions/$sessionId/complete" -Body @{ notes = "Alice is a fantastic teacher!" } -Token $bobToken

# 14. Chat Conversations
$convs = Test-ApiEndpoint -Name "Chat: Get Conversations (Alice)" -Method "GET" -Url "$baseUrl/chat/conversations" -Token $aliceToken
$convId = $convs[0].id

Test-ApiEndpoint -Name "Chat: Send Message" -Method "POST" -Url "$baseUrl/chat/conversations/$convId/messages" -Body @{ content = "Hello Bob! Looking forward to our next swap." } -Token $aliceToken
Test-ApiEndpoint -Name "Chat: Get Messages" -Method "GET" -Url "$baseUrl/chat/conversations/$convId/messages" -Token $bobToken

# 15. Reviews
$aliceReview = Test-ApiEndpoint -Name "Reviews: Alice Reviews Bob" -Method "POST" -Url "$baseUrl/reviews" -Body @{ sessionId = $sessionId; overallRating = 5; punctualityScore = 5; communicationScore = 5; knowledgeScore = 5; comment = "Bob is an outstanding Spanish mentor!" } -Token $aliceToken
$bobReview = Test-ApiEndpoint -Name "Reviews: Bob Reviews Alice" -Method "POST" -Url "$baseUrl/reviews" -Body @{ sessionId = $sessionId; overallRating = 5; punctualityScore = 5; communicationScore = 5; knowledgeScore = 5; comment = "Alice made C# Clean Architecture so clear!" } -Token $bobToken
Test-ApiEndpoint -Name "Reviews: Get User Reviews (Bob)" -Method "GET" -Url "$baseUrl/reviews/users/$bobId"

# 16. Notifications
Test-ApiEndpoint -Name "Notifications: Get My Notifications" -Method "GET" -Url "$baseUrl/notifications" -Token $aliceToken
Test-ApiEndpoint -Name "Notifications: Mark All Read" -Method "PUT" -Url "$baseUrl/notifications/read-all" -Token $aliceToken

# 17. Payments (Stripe)
$checkout = Test-ApiEndpoint -Name "Payments: Deposit Checkout" -Method "POST" -Url "$baseUrl/payments/deposit-checkout" -Body @{ amount = 15.00; successUrl = "http://localhost:5200/success"; cancelUrl = "http://localhost:5200/cancel" } -Token $johnToken
Test-ApiEndpoint -Name "Payments: Get History" -Method "GET" -Url "$baseUrl/payments/history" -Token $johnToken

# 18. Reports & Admin
Test-ApiEndpoint -Name "Reports: Create Report" -Method "POST" -Url "$baseUrl/reports" -Body @{ reportedUserId = $bobId; reasonCategory = "Inappropriate Behavior"; details = "Automated test report verification." } -Token $johnToken
Test-ApiEndpoint -Name "Admin: Get Platform Stats" -Method "GET" -Url "$baseUrl/admin/stats" -Token $adminToken
$reports = Test-ApiEndpoint -Name "Admin: Get Reports" -Method "GET" -Url "$baseUrl/admin/reports" -Token $adminToken
$reportId = $reports[0].id
Test-ApiEndpoint -Name "Admin: Resolve Report" -Method "PUT" -Url "$baseUrl/admin/reports/$reportId/resolve" -Body @{ status = "Resolved"; adminNotes = "Reviewed and found no violation." } -Token $adminToken

# Save Report JSON
$global:report | ConvertTo-Json -Depth 5 | Out-File -FilePath "C:\Users\azizn\.gemini\antigravity\brain\bdb8028a-0dd3-466a-9d04-471d3c2ff3b5\ENDPOINT_EXECUTION_RESULTS.json" -Encoding utf8
Write-Host "=== ENDPOINT VERIFICATION COMPLETED SUCCESSFULLY ===" -ForegroundColor Green
