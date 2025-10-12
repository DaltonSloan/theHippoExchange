// Creates the database and collections for HippoExchange
db = db.getSiblingDB(process.env.MONGO_INITDB_DATABASE || "HippoExchange");

// Users Collection
db.createCollection("users");
db.users.createIndex({ ClerkId: 1 }, { unique: true });

// Assets Collection
db.createCollection("assets");
db.assets.createIndex({ OwnerUserId: 1 });

// Maintenance Collection
db.createCollection("maintenance");
db.maintenance.createIndex({ AssetId: 1 });