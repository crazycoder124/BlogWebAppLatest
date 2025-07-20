
var connection = new signalR.HubConnectionBuilder().withUrl("/notificationHub").build();

connection.start().then(function () {
    
    console.log('connected to hub');
}).catch(function (err) {
    return console.error(err.toString());
});

connection.on("OnConnected", function () {
    OnConnected();
});

function OnConnected() {
    var username = $('.UserName').text().trim();
    var Userid = $(".UserId").text().trim();
    console.log(Userid);
    connection.invoke("SaveUserConnection", username, Userid).catch(function (err) {
        return console.error(" error in signalr",err.toString());
    })
}

connection.on("ReceivedNotification", function (notificatons) {
    //ReceivedPersonalNotification
    //console.log(message, 'General Message');
    debugger;
    $(".badge-number").text(notificatons[0].TotalNotification);
    console.log("Message hub", message);
    var $container = $(".notifications");
    $.each(notificatons, function (index, post) {

        var postHtml = `
                            <li class="notification-item" id="${post.id}">
                               <a href="/blog-details?id=${post.blogId}">
                                       <img src="/Uploads/${post.url}" alt="Profile" style="height:40px;width:40px;"  class="rounded-circle">
                                    <i class="bi bi-exclamation-circle text-success"></i>
                            <div>
                                    <h4>${post.username}</h4>
                                    <p>${post.body}</p>
                                    <p>${post.notificationDate}</p>
                            </div>
                            </a>
                        </li>

                        <li>
                            <hr class="dropdown-divider">
                        </li>
                `;

        $container.prepend(postHtml);
    });

});

connection.on("ReceivedPersonalNotification", function (notificatons) {
    //DisplayPersonalNotification(message, 'Hey ' + username); 
    console.log(notificatons);
    debugger
    $(".badge-number").text(notificatons[0].totalNotification);
    console.log("Message hub", message);
    var $container = $(".notifications");
    $.each(notificatons, function (index, post) {

        var postHtml = `
                            <li class="notification-item" id="${post.id}">
                               <a href="/blog-details?id=${post.blogId}">
                                       <img src="/Uploads/${post.url}" alt="Profile" style="height:40px;width:40px;"  class="rounded-circle">
                                    <i class="bi bi-exclamation-circle text-success"></i>
                            <div>
                                    <h4>${post.username}</h4>
                                    <p>${post.body}</p>
                                    <p>${post.notificationDate}</p>
                            </div>
                            </a>
                        </li>

                        <li>
                            <hr class="dropdown-divider">
                        </li>
                `;

        $container.prepend(postHtml);
    });
});

