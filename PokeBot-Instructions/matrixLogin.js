let chatTokenInput = document.getElementById('chatTokenInput');
let chatDomainInput = document.getElementById('chatDomainInput');
let submitButton = document.getElementById('submitButton');
let gzipData = document.getElementById('gzipData');

function getRawInfo(object) {
    return {
        serial: 13,
        next_batch: "",
        rooms: {},
        pinned_messages: {},
        room_invites: {},
        presences: {},
        login_data: object,
        account_data: {
            direct_rooms: {
                rooms: {}
            }
        },
        transaction_id: 1,
        ignored_users: {},
        room_events: {}
    };
}

function sendLoginRequest(){
    var requestUrl = `https://${chatDomainInput.value}/_matrix/client/v3/login`;
    var request = new XMLHttpRequest();
    request.open("POST", requestUrl);
    request.onreadystatechange = function() {
        if(request.readyState === XMLHttpRequest.DONE){
            var status = request.status;
            if (status === 0 || (status >= 200 && status < 400)) {
                var response = JSON.parse(request.responseText);
                var rawInfo = getRawInfo(response);
                let compressed_str = btoa(String.fromCharCode.apply(null, pako.gzip(JSON.stringify(rawInfo), {to: 'string'})));
                gzipData.value = compressed_str;
            } else {
                alert(`Error ${status}: ${request.responseText}`);
            }
        }
    }
    request.send(JSON.stringify({
        type: "org.matrix.login.jwt",
        token: chatTokenInput.value
    }));
}

submitButton.addEventListener("click", sendLoginRequest());