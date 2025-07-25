# 🥚 Fish Eggs - Universal Pond Stocking 🐟

*"Why wait for nature when you can play aquaculture god?"*

## What Does This Do?

Ever looked at a perfectly good pond and thought, "This needs more fish"? Well, now you can do something about it! This mod lets your colonists stock any water source with fish using... fish eggs! Revolutionary, right?

## Features

🎯 **Right-Click & Stock** - Click fish eggs, target water, watch magic happen  
🌊 **Works Everywhere** - Freshwater, saltwater, that weird puddle behind your workshop  
🐠 **Smart Fish Matching** - System refuses to stock incompatible fish types  
🎣 **Full VFE Integration** - Works seamlessly with Vanilla Fishing Expanded  
⚙️ **Configurable Everything** - Tweak settings until it feels just right  
🛒 **Trader Goods** - Buy fish eggs from merchants

## How It Works

1. Get some fish eggs (traders sell them, or... *creative acquisition*)
2. Right-click an egg → "Seed Water Source"  
3. Click on any water tile with the fancy targeting cursor
4. Congratulations, you're now a fish farmer!
5. Cast your fishing rod and enjoy your artificially enhanced ecosystem

## Requirements

- RimWorld 1.6
- Vanilla Fishing Expanded (the fish need somewhere to swim to)

## Balance Notes

- Fish eggs rot after 5 days (fish don't wait forever)
- Water type matters (salmon are smart, but not *that* smart)  
- Stocked fish integrate with vanilla population mechanics
- Your colonists get Animals XP for successful seeding (fish whispering is a skill)

## Compatibility

✅ Safe to add to existing saves  
✅ Works with most water-related mods  
✅ VFE integration tested extensively  
🤔 May cause spontaneous aquaculture addiction  

## Recent Fixes

### v1.0.2 - Job Count Fix
- **Fixed:** "Invalid count: -1" error when seeding water sources
- **Issue:** Job creation was not setting the count parameter, causing hauling errors
- **Resolution:** Set job.count = 1 when creating SeedWaterSource jobs

### v1.0.1 - XML Format Fix
- **Fixed:** TraderKinds XML parsing error with IntRange format  
- **Issue:** Range values were using hyphens (`3-8`) instead of tildes (`3~8`)  
- **Resolution:** Updated all IntRange values to use proper tilde format for RimWorld's parser

---

*"Because sometimes the best solution to not having fish is just adding more fish."* 🐟