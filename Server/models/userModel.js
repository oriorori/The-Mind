const { ObjectId } = require("mongodb");

exports.findUserByKey = async (key) => {
  const db = global.db.collection("users");
  return await db.findOne({ _id: new ObjectId(key) });
};

exports.findUserById = async (userId) => {
  const db = global.db.collection("users");
  return await db.findOne({ userId: userId });
};

exports.updateUser = async (userId, updatedUser) => {
  const db = global.db.collection("users");
  await db.updateOne({ userId: userId }, { $set: updatedUser });
  return updatedUser;
};

exports.insertUser = async (userId, hash, nickname) => {
  const db = global.db.collection("users");
  return await db.insertOne({
    userId: userId,
    password: hash,
    nickname: nickname,
    coin: 0,
    winCount: 0,
    loseCount: 0,
    waitingSecondPerNumber: 0.0,
    totalPlayedCard: 0,
    unlockedCardBack: ["default"]
  });
};
