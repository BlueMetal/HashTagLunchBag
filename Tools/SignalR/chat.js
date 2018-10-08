// The following sample code uses modern ECMAScript 6 features 
// that aren't supported in Internet Explorer 11.
// To convert the sample for environments that do not support ECMAScript 6, 
// such as Internet Explorer 11, use a transpiler such as 
// Babel at http://babeljs.io/. 
//
// See Es5-chat.js for a Babel transpiled version of the following code:

const currentEvent = 'LasVegas';
var connection = null;

function connectSignalR(){
	connection = new signalR.HubConnectionBuilder()
		.withUrl("https://lunchbagwebportal.eastus.cloudapp.azure.com/api/signalr")
		.configureLogging(signalR.LogLevel.Information)
		.build();
	//connection.serverTimeoutInMilliseconds = 5000; // 5 second
	
	connection.on("ReceiveGoalUpdate", (message) => {
		console.log('Receiving ReceiveGoalUpdate...');
		console.log(message);
	});

	connection.on("ReceiveSentiment", (message) => {
		console.log('Invoking ReceiveSentiment...');
		console.log(message);
	});
	
	connection.on("ReceiveNote", (message) => {
		console.log('Invoking ReceiveNote...');
		console.log(message);
    });

    connection.on("ReceiveDeliveryUpdate", (message) => {
        console.log('Invoking ReceiveDeliveryUpdate...');
        console.log(message);
    });

	connection.start().catch(err => console.error(err.toString())).then(requestUpdate);
}

function requestUpdate(){
	console.log('Invoking InitConnection...');
	connection.invoke("InitConnection", "", currentEvent).catch(err => console.error(err.toString())).then(readyCallBack);
}

function readyCallBack(){
	console.log('Ready to show the page');
}


connectSignalR();