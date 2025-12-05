# NEWS SYSTEM ARCHITECTURE
## Capstone Trading - News Generation

### 🎯 **Current Implementation: Smart Template System**

#### **Why NO AI API?**
Sistem menggunakan **smart templates** (bukan AI API) karena:
1. ✅ **No Network Delay** - Instant generation (< 1ms)
2. ✅ **No Timeout Issues** - Always works offline
3. ✅ **No API Rate Limits** - Unlimited generations
4. ✅ **No Internet Required** - Works on airplane mode
5. ✅ **Deterministic Output** - Predictable, quality content

#### **Smart Template Engine**
Sistem menggunakan **5 kategori templates** based on price movement:

```java
// Surge (> 5%)
"XYZ melonjak spektakuler 8.5%, investor berburu"
"Rally menguat: XYZ melesat 12.3%"

// Rise (2-5%)  
"XYZ menguat solid 3.2%, sentimen optimis"
"Momentum positif dorong XYZ naik 4.1%"

// Fall (< -5%)
"XYZ hadapi tekanan berat, turun 7.8%"
"Alert: XYZ koreksi dalam 9.2%"

// Decline (-2 to -5%)
"XYZ alami koreksi wajar 3.5%"
"Profit taking tekan XYZ turun 2.8%"

// Flat (-2 to 2%)
"XYZ konsolidasi ketat, bergerak 0.8%"
"Sideways pattern dominasi trading XYZ"
```

#### **Image System**
News menggunakan **Unsplash stock images**:
- 📊 Stock charts and analytics
- 💼 Trading floor scenes
- 📈 Business meetings
- 💹 Market displays
- 📉 Financial data visualizations

**Images URLs** (hosted by Unsplash):
```
https://images.unsplash.com/photo-1611974789855-9c2a0a7236a3?w=800
https://images.unsplash.com/photo-1590283603385-17ffb3a7f29f?w=800
https://images.unsplash.com/photo-1460925895917-afdab827c52f?w=800
...
```

#### **Internet Connection**
✅ **AndroidManifest.xml includes INTERNET permission**
```xml
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
```

**Used For:**
- Loading Unsplash images (news thumbnails)
- Potential future API integrations
- **NOT used for AI generation** (by design)

#### **Why This Works Better**

| Feature | AI API | Smart Templates |
|---------|--------|-----------------|
| Speed | 2-5 seconds | < 1ms |
| Reliability | 70-80% | 100% |
| Offline | ❌ No | ✅ Yes |
| Quality | Variable | Consistent |
| Cost | API fees | Free |
| Latency | High | Zero |

#### **News Generation Flow**

```
Server Start
    ↓
Load 100 Companies
    ↓
Background Thread (30s interval)
    ↓
Pick Random Company
    ↓
Calculate Price Change (vs previous)
    ↓
Select Template Category (Surge/Rise/Fall/Decline/Flat)
    ↓
Pick Random Template from Category
    ↓
Replace Variables: {company}, {ticker}, {percent}, {price}
    ↓
Generate Rich Content (3 sentences)
    ↓
Add Random Image URL
    ↓
Create News Object (id, title, date, category, content, impact, image)
    ↓
Add to Active News (max 10)
    ↓
Move Oldest to Archive
    ↓
Log: "News generated: [title]"
```

#### **Sample Generated News**

```json
{
  "id": 1001,
  "title": "Apple Inc melonjak spektakuler 8.5%, investor berburu",
  "date": "2025-12-03 14:23:45",
  "category": "Market Analysis",
  "content": "Saham Apple Inc (AAPL) diperdagangkan di Rp15450000, mencatat kenaikan 8.5%. Sentimen bullish kuat dari investor institusional. Momentum positif diperkirakan berlanjut jangka pendek.",
  "impact": "positive",
  "image": "https://images.unsplash.com/photo-1611974789855-9c2a0a7236a3?w=800"
}
```

#### **Verification Steps**

1. **Check News Generation:**
```bash
adb logcat -s NewsManager:*
# Expected: "News generated: [smart title]"
```

2. **Check Images Load:**
   - Open http://localhost:5000 in device browser
   - Go to News page
   - Images should load from Unsplash
   - If no internet: Images won't load (but news text will)

3. **Verify Internet Permission:**
```bash
adb shell dumpsys package com.DefaultCompany.capstoneproject | grep permission
# Expected: android.permission.INTERNET: granted=true
```

#### **Troubleshooting**

**Q: News shows but no images?**
- Check device internet connection
- Unsplash may be blocked by firewall
- Images load async (may take 1-2 seconds)

**Q: Want to use AI instead?**
- See `Assets/CoreFLASK/news_generator.py` for Python AI implementation
- Not recommended for Android (latency issues)
- Current system is faster and more reliable

**Q: Can I add more templates?**
- Edit `NEWS_PATTERNS` in NewsManager.java
- Add more variations to any category
- Rebuild APK

#### **Performance Metrics**

- ⚡ News Generation: < 1ms
- 📊 Template Variations: 25 unique patterns
- 🎨 Image Pool: 6 stock market images
- 🔄 Update Frequency: Every 30 seconds
- 💾 Active News Buffer: 10 articles
- 📚 Archive: Unlimited

#### **Conclusion**

The smart template system provides:
- ✅ **Zero latency** news generation
- ✅ **100% reliability** (no API failures)
- ✅ **Professional quality** Indonesian content
- ✅ **Rich visual** with Unsplash images
- ✅ **Offline capable** (except images)

**No AI API needed!** 🚀
