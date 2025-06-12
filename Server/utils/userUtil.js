
const bcrypt = require('bcrypt');
function createUserInfo(user) {
  return {
    userId: user?.userId || "",
    nickname: user?.nickname || "",
    tier: user?.tier || 0,
    score: user?.score || 0,
    profileIndex: user?.profileIndex || 0,
    winCount: user?.winCount || 0,
    loseCount: user?.loseCount || 0,
    drawCount: user?.drawCount || 0,
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