// Creates an application user (optional if using only root in dev)
db = db.getSiblingDB(process.env.MONGO_INITDB_DATABASE || "HypoExchange");
db.createCollection("profiles");
db.profiles.createIndex({ UserId: 1 }, { unique: true });
