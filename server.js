const express = require("express");
const {createServer} = require("http");
const { url } = require("inspector");
const {Server} = require("socket.io");
const app = express();
const appServer = new createServer(app);
const io = new Server(appServer);
var usernames = {};
app.use(express.static(__dirname + "/public"));
app.get("/", (req, res)=>{
 res.sendFile(__dirname + "/public/index.html");
});
io.on('connection', function(socket){
    console.log('User Connected...')
    socket.on('message', (msg) => {
       socket.to(socket.room).emit('message', msg);
    });  
    socket.on('adduser', function(username, roomname){
        socket.join(roomname);
        socket.room = roomname;
		socket.username = username;
		usernames[username+"_"+roomname] = username;
        io.sockets.in(socket.room).emit('updateusers', usernames);
        socket.emit('greeting', username );
	}); 
	 socket.on('uploadImage', function (data, username) {
        socket.to(socket.room).emit('publishImage', data, username);
    });
    socket.on('disconnect', function(){
        console.log("User Disconnected");
        delete usernames[socket.username + '_' + socket.room];
        socket.leave(socket.room);
        socket.to(socket.room).emit('updateusers', usernames);
    })
})
appServer.listen(7070, ()=>{
    console.log("server is running at http://localhost:7070");
})