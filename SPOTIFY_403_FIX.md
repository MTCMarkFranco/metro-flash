# Fixing Spotify 403 Forbidden Error

You're getting a **403 Forbidden** error when trying to access the audio-features endpoint. This is a common issue with Spotify apps in **Development Mode**.

## The Problem

Spotify restricts certain API endpoints (including audio-features) for apps in Development Mode to prevent abuse. Your app needs to be properly configured to access these endpoints.

## Solution: Add Your Account to the Allowlist

1. **Go to Spotify Developer Dashboard**
   - Visit: https://developer.spotify.com/dashboard
   - Click on your app (Visual Metronome)

2. **Add Your Email to Allowlist**
   - Click on **"Users and Access"** in the left sidebar
   - Click **"Add New User"**
   - Enter your Spotify account email address (the one you use to log into Spotify)
   - Click **"Add"**

3. **Verify App Settings**
   - Go to **"Settings"** 
   - Verify the Redirect URI is exactly: `http://127.0.0.1:5000/callback`
   - Make sure the app status shows **"In Development"**

4. **Re-authenticate**
   - Close your Visual Metronome app completely
   - Run the app again: `dotnet run --project VisualMetronome.csproj`
   - Click "Connect to Spotify" and authorize again
   - The new permissions should now work

## Alternative: Request Extended Quota Mode

If you plan to share this app with others, you'll need to request Extended Quota Mode:

1. In your Spotify app dashboard, click **"Request Extension"**
2. Fill out the form explaining your use case
3. Wait for Spotify's approval (can take several days)

Once approved, anyone can use your app without being on the allowlist.

## Verification

After adding your email to the allowlist and re-authenticating, run the app and check the console output. You should see:
- Authentication success message with token details
- No more 403 errors when loading songs
- BPM values successfully retrieved

## Still Having Issues?

If you still get 403 errors after following these steps:
1. Make sure you're logged into Spotify with the same email you added to the allowlist
2. Try logging out of Spotify completely and logging back in
3. Clear your browser cookies and re-authenticate
4. Wait 5-10 minutes for Spotify's systems to propagate the changes
