var directLine = new DirectLine.DirectLine({
    secret: "NtlPAy0ijLM.cwA.5ww.nkzOFYavLJPrf5UhLWi7rB8ZYpapzYVu0C0dbs6wx1g"
});

var bingClientTTS = new BingTTS.Client("ec36dce7cf73494d8208b1ec284218ef", "enGB_Female");

var client = new BingSpeech.RecognitionClient("ec36dce7cf73494d8208b1ec284218ef", "en-GB");
client.startMicAndContinuousRecognition();

client.onFinalResponseReceived = function (response) {
    // $("#text-display").html(response);
    // lastQuestion = response;
    $("#chatwindow").prepend("<div class='dialogbubbleright'><i><b>You: </b></i>" + response + "</div>"); 
    directLine.postActivity({
        from: { id: '1234', name: 'Tyler' }, // required (from.name is optional)
        type: 'message',
        text: response
    }).subscribe(
        id => console.log("Posted activity, assigned ID ", id),
        error => console.log("Error posting activity", error)
    );     
}
client.onError = function (code, requestId) {
    console.log("<Error with request n°" + requestId + ">");
}
client.onVoiceDetected = function () {
    //speechActivity.classList.remove("hidden");
}
client.onVoiceEnded = function () {
    //speechActivity.classList.add("hidden");
}
client.onNetworkActivityStarted = function () {
    //networkActivity.classList.remove("hidden");
}
client.onNetworkActivityEnded = function () {
    //networkActivity.classList.add("hidden");
}

// directLine.postActivity({
//     from: { id: '1234', name: 'Tyler' }, // required (from.name is optional)
//     type: 'message',
//     text: 'wie ändere ich die sprache?'
// }).subscribe(
//     id => console.log("Posted activity, assigned ID ", id),
//     error => console.log("Error posting activity", error)
// );

directLine.activity$
.subscribe(
    activity => {
        //$("#text-display").html(activity.text);
        if (activity.from.id == "FloweyBot" && activity.text != "") {
            
            bingClientTTS.synthesize(activity.speak, BingTTS.SupportedLocales.enGB_Female, () => {
                
            });
            $("#chatwindow").prepend("<div class='dialogbubbleleft'><i><b>CobiBot: </b></i> " + activity.speak + "</div>");    
        }
        // else {
        //     $("#chatwindow").prepend("<div class='dialogbubbleright'><i>You</i>" + activity.text + "</div>");    
        // }
    } 
);

var lastQuestion = "";

// A $( document ).ready() block.
$(document).ready(function() {
    $("#textInput").keypress(function(key) {
        if (key.keyCode == 13) {
            var x = document.getElementById("textInput").value;
            document.getElementById("textInput").value = "";
            $("#chatwindow").prepend("<div class='dialogbubbleright'><i><b>You: </b></i>" + x + "</div>"); 
            // lastQuestion = x;
            directLine.postActivity({
                from: { id: '1234', name: 'Tyler' }, // required (from.name is optional)
                type: 'message',
                text: x
            }).subscribe(
                id => console.log("Posted activity, assigned ID ", id),
                error => console.log("Error posting activity", error)
            );            
        }
    });
});