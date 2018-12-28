require('dotenv').config();

var amqp = require('amqp'),
    app = require('express')(),
    http = require('http').Server(app),
    io = require('socket.io')(http);

rabbitMq = amqp.createConnection({
    host: process.env.RMQ_HOST,
    port: process.env.RMQ_PORT,
    login: process.env.RMQ_USER,
    password: process.env.RMQ_PASS,
    connectionTimeout: Number(process.env.RMQ_TIMEOUT),
    authMechanism: 'AMQPLAIN',
    vhost: process.env.RMQ_VHOST,
    noDelay: true,
    ssl: {
        enabled: false
    }
}, {
    defaultExchangeName: "Topic_Exchange"
});

app.get('/', (req, res) => {
    res.sendFile(__dirname + '/index.html');
});

app.set('port', process.env.PORT);

var q = 'CardPaymentTopic_Queue';
var routingKey = 'watchlist.dashboard';
var channel = 'chat message';

rabbitMq.on('error', (e) => {
    console.log('EROR: ' + e)
});

rabbitMq.on('ready', () => {
    console.log('Connected to RabbitMQ');
    rabbitMq.queue(q, {
        autoDelete: false,
        durable: true,
        exclusive: false
    }, (q) => {
        //q.bind('#'); // Catch all messages   
        //q.bind('Topic_Exchange', 'payment.cardpayment');
        q.subscribe(function (message) {
            console.log('[+] ********************************************************')
            console.log('[+] Name : ' + q.name + ', EventsCount: ' + q.currentMessage.queue._eventsCount);
            console.log('[+] Exchange : ' + q.currentMessage.exchange + ', RoutingKey : ' + q.currentMessage.routingKey);
            console.log('[+] Timestamp : ' + q.currentMessage.headers.Timestamp);

            obj = JSON.parse(message.data.toString());
            //socket.broadcast.to(obj.id).emit('message', obj);
            //io.sockets.in(obj.id).emit('message', obj);

            let msgJson = JSON.stringify(obj, undefined, 2);
            if (q.currentMessage.routingKey === routingKey) {
                // Send Message to Client
                io.emit(channel, msgJson);
                console.log(msgJson);
            }
        });
    });
});

io.on('connection', (socket) => {
    console.log('Client connected (Socket IO)');
    console.log('Client Id: ' + socket.id);
    socket.on(channel, (msg) => {
        console.log('message:' + msg);
        // Reply
        io.emit(channel, msg);
    });
});

http.listen(process.env.PORT, () => {
    console.log('Server start on http://127.0.0.1:' + process.env.PORT);
});