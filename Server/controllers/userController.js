const userModel = require("../models/userModel");
const { createUserInfo, hashPassword, comparePassword,} = require("../utils/userUtil");

async function signUp(req, res){
  try {
    const { userId, nickname, password, passwordConfirmation } = req.body;

    if (!userId || !password || !passwordConfirmation || !nickname) {
      return res.status(400).json({ error: '필수 값 없음' });
    }

    if (password !== passwordConfirmation) {
      return res.status(401).json({ error: '비밀번호가 일치하지 않음' });
    }

    const existingUser = await userModel.findUserById(userId);
    // 플레이어 ID가 이미 존재하는지 확인
    if (existingUser) {
      return res.status(409).json({ error: '이미 가입된 플레이어입니다.' });
    }

    var hash = hashPassword(password);
    // 새로운 플레이어를 데이터베이스에 추가
    await userModel.insertUser(userId, hash, nickname);
    console.log(`🟢 ${userId}님이 가입했어요`);
    return res.status(201).json({ message: '가입 성공' });
  } catch (error) {
    console.error('가입 에러:', error);
    return res.status(500).json({ error: 'Internal server error' });
  }
}

async function signIn(req, res) {
  try {
    const { userId, password } = req.body;

    if (!userId || !password) {
      return res.status(400).json({ error: '필수 값 없음' });
    }

    // 플레이어 ID가 존재하는지 확인
    const existingUser = await userModel.findUserById(userId);
    if( !existingUser) {
      return res.status(404).json({ error: '존재하지 않는 플레이어입니다.' });
    }
    else{
        // 비밀번호 확인
        const isPasswordValid = comparePassword(password, existingUser.password);
        if (!isPasswordValid) {
          return res.status(401).json({ error: '비밀번호가 일치하지 않음' });
        }

        req.session.isAuthenticated = true;
        req.session.userId = existingUser._id.toString();
        req.session.username = existingUser.username;
        req.session.nickname = existingUser.nickname;

        console.log(`🟢 ${userId}님이 로그인했어요`);

        var userInfo = createUserInfo(existingUser);

        return res.status(200).json({userInfo: userInfo});
    }

  } catch (error) {
    console.error('로그인 에러:', error);
    return res.status(500).json({ error: 'Internal server error' });
  }
}

function signOut(req, res) {
    req.session.destroy((err) => {
      if (err) {
        console.log("로그아웃 중 오류 발생");
        return res.status(500).send("서버 오류가 발생했습니다.");
      }
      res.status(200).send("로그아웃 되었습니다.");
    });
  }
  

module.exports = {
    signUp,
    signIn,
    signOut
  };