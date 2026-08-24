# 🚀 ZAH Backend - Render.com Deployment (100% FREE)

## ✅ Why Render?
- **NO credit card required**
- **100% FREE tier** (750 hours/month)
- **Super easy** - deploy in 5 minutes
- **Auto-deploys** from GitHub
- Perfect for startups and small projects

---

## 📋 Prerequisites

### 1. GitHub Account
Create one at: https://github.com/signup (if you don't have)

### 2. MongoDB Atlas (FREE Database)
1. Go to: https://www.mongodb.com/cloud/atlas/register
2. Sign up (FREE - no card needed)
3. Create **FREE M0 Cluster** (512MB)
4. Setup:
   - **Database Access:** Create user (username/password)
   - **Network Access:** Add IP `0.0.0.0/0` (allow all)
5. Get connection string:
   - Click "Connect" → "Drivers"
   - Copy: `mongodb+srv://username:password@cluster0.xxxxx.mongodb.net/zah_ecommerce`

---

## 🔧 STEP-BY-STEP DEPLOYMENT

### **STEP 1: Push Code to GitHub**

Open terminal in your backend folder:

```cmd
cd "c:\Users\Mathi\OneDrive\Desktop\ZAHGO-BACKEND\zah-backend"

git init
git add .
git commit -m "Initial commit - ZAH Backend"
```

Then create a repository on GitHub:
1. Go to: https://github.com/new
2. Repository name: `zah-backend`
3. Make it **Private**
4. Click "Create repository"

Push your code:
```cmd
git remote add origin https://github.com/YOUR-USERNAME/zah-backend.git
git branch -M main
git push -u origin main
```

---

### **STEP 2: Sign Up on Render**

1. Go to: https://render.com/
2. Click **"Get Started for Free"**
3. Sign up with **GitHub** (easiest)
4. Authorize Render to access your repositories

---

### **STEP 3: Deploy Your Backend**

1. In Render dashboard, click **"New +"** → **"Web Service"**

2. Connect your GitHub repository:
   - Find `zah-backend` repository
   - Click **"Connect"**

3. Configure the service:
   - **Name:** `zahgo-backend`
   - **Region:** Oregon (US West) or closest to you
   - **Branch:** `main`
   - **Runtime:** Docker
   - **Plan:** **FREE**

4. Render will auto-detect your `Dockerfile` and `render.yaml`

5. Click **"Create Web Service"**

6. Wait 5-10 minutes for deployment...

---

### **STEP 4: Set Environment Variables**

Once deployed, go to your service dashboard:

1. Click **"Environment"** tab on the left

2. Add these variables:

**Variable 1:**
```
Key:   MONGODB_CONNECTION_STRING
Value: mongodb+srv://username:password@cluster0.xxxxx.mongodb.net/zah_ecommerce
```

**Variable 2:**
```
Key:   JWT_SECRET
Value: your-super-secret-key-minimum-32-characters-long
```

3. Click **"Save Changes"**

4. Your app will auto-redeploy (2-3 minutes)

---

### **STEP 5: Test Your Backend**

Your backend URL will be:
```
https://zahgo-backend.onrender.com
```

Test these endpoints:

1. **Health Check:**
   ```
   https://zahgo-backend.onrender.com/health
   ```

2. **Swagger Documentation:**
   ```
   https://zahgo-backend.onrender.com/swagger
   ```

3. **Get Categories:**
   ```
   https://zahgo-backend.onrender.com/api/v1/categories
   ```

---

### **STEP 6: Connect to Your Frontend**

Update your Angular app's environment file:

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://zahgo-backend.onrender.com/api/v1'
};
```

---

## ⚠️ Important Notes

### Free Tier Limitations:
- ✅ **750 hours/month** (enough for small traffic)
- ⚠️ **App sleeps after 15 minutes of inactivity**
- ⏰ **Takes ~30 seconds to wake up** on first request
- ✅ Perfect for development and small projects

### To avoid sleep (optional):
- Use a service like **UptimeRobot** (free) to ping your API every 14 minutes
- Or upgrade to paid plan ($7/month for always-on)

---

## 🔄 Update Your Backend

After making code changes:

```cmd
git add .
git commit -m "Updated feature"
git push origin main
```

Render will **auto-deploy** your changes!

---

## 🐛 Troubleshooting

### Check Logs:
1. Go to Render dashboard
2. Click your service
3. Click "Logs" tab
4. View real-time logs

### Common Issues:

**MongoDB Connection Failed:**
- Check connection string
- Verify IP whitelist includes `0.0.0.0/0`
- Ensure username/password is correct

**App Not Starting:**
- Check logs for errors
- Verify Dockerfile is correct
- Ensure environment variables are set

---

## 💰 Costs

**100% FREE for:**
- 750 hours/month
- 512 MB RAM
- MongoDB Atlas FREE tier (512MB)

**Total: $0/month** 🎉

---

## ✅ Deployment Checklist

- [ ] Code pushed to GitHub
- [ ] MongoDB Atlas setup complete
- [ ] Render account created
- [ ] Backend deployed on Render
- [ ] Environment variables set
- [ ] Health check returns 200 OK
- [ ] Swagger UI accessible
- [ ] Frontend connected
- [ ] Test API endpoints working

---

**🎉 Congratulations! Your backend is live on Render (FREE)!**

Need help? Check the logs or let me know!
