
const bcrypt = require('bcrypt');
function createUserInfo(user) {
  return {
    userId: user?.userId || "",
    nickname: user?.nickname || "",
    coin: user?.coin || 0,
    winCount: user?.winCount || 0,
    loseCount: user?.loseCount || 0,
    waitingSecondPerNumber: user?.waitingSecondPerNumber || 0.0,
    totalPlayedCard: user?.totalPlayedCard || 0,
    unlockedCardBack: user?.unlockedCardBack || [0]
  };
}
function hashPassword(password, saltRounds = 10) {
  const salt = bcrypt.genSaltSync(saltRounds);
  return bcrypt.hashSync(password, salt);
}
function comparePassword(inputPassword, hashedPassword) {
  return bcrypt.compareSync(inputPassword, hashedPassword);
}
module.exports = { createUserInfo, hashPassword, comparePassword };