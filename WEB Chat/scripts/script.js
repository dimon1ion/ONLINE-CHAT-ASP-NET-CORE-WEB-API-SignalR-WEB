class User {
  name;
  connectionId;
  constructor(name, connectionId) {
    this.name = name;
    this.connectionId = connectionId;
  }
}

var serverUrl = "http://localhost:38058/chat";
var usersOnline = [];
var privateUsers = [];

checkToken();

const connection = new signalR.HubConnectionBuilder()
  .configureLogging(signalR.LogLevel.None)
  .withUrl(serverUrl, {
    accessTokenFactory: () => getCookie("token"),
  })
  .withAutomaticReconnect()
  .build();

connectionStart();

function connectionStart() {
  connection.on("AllReceive", (user, message, toMe) => {
    let messages = document.getElementById("globalMessages");

    let alertColor = "alert-primary";
    if (toMe) {
      alertColor = "alert-info";
    }

    let result = document.createElement("div");
    result.classList.add("alert", alertColor);
    result.innerHTML = `<b>${user}: </b> ${message}`;

    messages.appendChild(result);
  });

  connection.on("PrivateReceive", (user, message, recipients) => {
    let to = "";
    if (recipients !== undefined) {
      to += "to -> ";
      for (let i in recipients) {
        to += `${recipients[i]}`;
        if (i != recipients.count - 1) {
          to += ", ";
        }
      }
    }
    let messages = document.getElementById("privateMessages");

    let result = document.createElement("div");
    result.classList.add("alert", "alert-warning");
    result.innerHTML = `<b>${user} ${to}: </b> ${message}`;

    messages.appendChild(result);
  });

  connection.on("UsersOnline", (users) => {
    let userList = document.getElementById("users");
    console.log(users);
    for(user of users){
      console.log(user);
      usersOnline[usersOnline.length] = new User(user.name, user.connectionId);
      let newUser = document.createElement("div");
      newUser.classList.add(
        "alert",
        "alert-info",
        "justify-content-between",
        "d-flex",
        `name-${user.Name}`
      );
      newUser.innerHTML = `<b>${user.name}</b> <button type="button" class="btn btn-primary w-auto" onclick="addPrivateUser(event)">+</button>`;
      userList.appendChild(newUser);
    }
  });

  connection.on("UserConnect", (name, connectionId, connected) => {
    let users = document.getElementById("users");
    if (connected) {
      usersOnline[usersOnline.length] = new User(name, connectionId);
      let user = document.createElement("div");
      user.classList.add(
        "alert",
        "alert-info",
        "justify-content-between",
        "d-flex",
        `name-${name}`
      );
      user.innerHTML = `<b>${name}</b> <button type="button" class="btn btn-primary w-auto" onclick="addPrivateUser(event)">+</button>`;
      users.appendChild(user);
    } else {
      let deletedUser;
      for (i in usersOnline) {
        if (
          usersOnline[i].name == name &&
          usersOnline[i].connectionId == connectionId
        ) {
          deletedUser = usersOnline.splice(i, 1)[0];
          break;
        }
      }
      if (deletedUser === undefined) {
        return;
      }
      let elements = document.getElementsByClassName(
        `name-${deletedUser.name}`
      );
      for (e of elements) {
        e.remove();
      }
    }
  });

  // connection.on("OnConnected", () =>{
  //   let token = parseJwt(getCookie("token"));
  //   let name = token["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"];
  //   let message = `User ${name} has connected!`;

  //   connection.invoke("NotifyAsync", message);
  // });
  if (notification !== undefined) {
    connection.on("Notify", (message, date) => {
      const notify = document.getElementById("notification");

      notifyMessage = `<div class="toast-header">
    <strong class="me-auto">Notification</strong>
    <small>${date}</small>
    <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
    </div>
    <div class="toast-body">
    ${message}
    </div>`;

      notify.innerHTML = notifyMessage;

      const toast = new bootstrap.Toast(notify);

      toast.show();
    });
  }

  connectOn();
}

function sendMessage() {
  if (connection === undefined) {
    return;
  }
  let message = document.getElementById("message").value;

  connection.invoke("SendAllUsers", message);
}

function sendPrivateMessage() {
  if (connection === undefined) {
    return;
  }
  let message = document.getElementById("message").value;
  if (privateUsers.length > 0) {
    let userIds = [];
    let userNames = [];
    for(user of privateUsers){
      userNames[userNames.length] = user.name;
      userIds[userIds.length] = user.connectionId;
    }
    connection.invoke("SendPrivateUsers", message, userNames, userIds);
  }
  else{
    alert("choose user(s)");
  }
}

function deleteToken() {
  deleteCookie("token");
  checkToken();
}

function onConnectionErrorAsync() {
  let interval = setInterval(function () {
    switch (connection.state) {
      case "Disconnected":
        deleteToken();
      case "Connected":
        clearInterval(interval);
        break;
    }
  }, 1000);
}

function connectOn() {
  connection.start();
  onConnectionErrorAsync();
}

// onConnectionError();

// console.log(connection.state);
// for(;connection.state == "Connecting";){

// }
// console.log(connection.state);

function checkToken() {
  if (getCookie("token") === "") {
    window.open("sign.html", "_self");
  }
}

function getCookie(cname) {
  let name = cname + "=";
  let decodedCookie = decodeURIComponent(document.cookie);
  let ca = decodedCookie.split(";");
  for (let i = 0; i < ca.length; i++) {
    let c = ca[i];
    while (c.charAt(0) == " ") {
      c = c.substring(1);
    }
    if (c.indexOf(name) == 0) {
      return c.substring(name.length, c.length);
    }
  }
  return "";
}

function deleteCookie(cname) {
  let expireDate = new Date(2000, 1);
  document.cookie =
    cname +
    "=" +
    ";domain=" +
    window.location.hostname +
    ";path=/" +
    ";expires=" +
    expireDate.toUTCString();
}

function parseJwt(token) {
  var base64Url = token.split(".")[1];
  var base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
  var jsonPayload = decodeURIComponent(
    atob(base64)
      .split("")
      .map(function (c) {
        return "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2);
      })
      .join("")
  );

  return JSON.parse(jsonPayload);
}

function addPrivateUser(event) {
  let name = event.target.parentElement.firstChild.innerText;
  for (user of usersOnline) {
    if (user.name == name) {
      let privateTo = document.getElementById("privateToUsers");
      if (privateTo.getElementsByClassName(`name-${user.name}`).length !== 0) {
        return;
      }
      privateUsers[privateUsers.length] = user;
      let element = document.createElement("button");
      element.classList.add("alert", "alert-info", `name-${user.name}`);
      element.addEventListener("pointerdown", deletePrivate);
      element.innerHTML = `<b>${user.name}</b>`;
      privateTo.appendChild(element);
      break;
    }
  }
}

function deletePrivate(event){
  let privateTo = document.getElementById("privateToUsers");
  let name = event.target.innerText;
  for(i in privateUsers){
    if (privateUsers[i].name == name) {
      let elements = privateTo.getElementsByClassName(`name-${privateUsers[i].name}`);
      for(element of elements){
        element.remove();
      }
      privateUsers.splice(i,1);
    }
  }
}