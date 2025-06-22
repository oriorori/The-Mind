var mongodb = require("mongodb");
var MongoClient = mongodb.MongoClient;

async function connectDB(callback) {
  //var databaseURL = "mongodb://root:1234@localhost:27017/";
  const databaseURL =
    "mongodb+srv://dbuser:0000@cluster0.h1de8gc.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";

  try {
    const database = await MongoClient.connect(databaseURL, {
      useNewUrlParser: true,
      useUnifiedTopology: true,
      tls: true,
      tlsAllowInvalidCertificates: true,
    }).then((client) => {
      const db = client.db("gamedb");
      //app.set('database', db);
      global.db = db;
      callback();
      console.log("DB 연결 성공");

      // 연결 종료 처리
      process.on("SIGINT", async () => {
        await client.close();
        console.log("DB 연결 종료");
        process.exit(0);
      });
    });
  } catch (err) {
    console.error("DB 연결 실패: " + err);
    process.exit(1);
  }
}

module.exports = connectDB;
