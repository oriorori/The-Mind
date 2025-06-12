var express = require('express');
var router = express.Router();

const roomController = require('../controllers/roomController');
const userController = require('../controllers/userController');

router.post('/signUp', userController.signUp);
router.post('/signIn', userController.signIn);
router.post('/createRoom', roomController.createRoom);
router.post('/joinRoom', roomController.joinRoom);
router.post('/destroyRoom', roomController.destroyRoom);

module.exports = router;