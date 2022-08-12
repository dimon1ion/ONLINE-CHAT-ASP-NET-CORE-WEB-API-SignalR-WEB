// if (getCookie("token") !== "") {
//   window.open("index.html", "_self");
// }
function check() {
  const urlParams = new URLSearchParams(window.location.search);
  const authType = urlParams.get("type");
  if (authType === "auth") {
    registrationUser.classList.remove("d-none");
  } else {
    loginUser.classList.remove("d-none");
  }
}

function changeToLogin() {
  registrationUser.classList.add("d-none");
  loginUser.classList.remove("d-none");
  // location.search = "";
}
function changeToReg() {
  loginUser.classList.add("d-none");
  registrationUser.classList.remove("d-none");
  // location.search = "?type=auth";
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

async function login(e) {
  e.target.classList.add("d-none");
  loadingLog.classList.remove("d-none");
  validErrorsLog.innerHTML = "";
  let user = {
    name: document.getElementById("userName2").value,
    password: document.getElementById("pass2").value,
  };

  let response;
  try {
    response = await fetch("http://localhost:38058/Auth/Login", {
      method: "POST",
      body: JSON.stringify(user),
      headers: {
        Authorization: `Bearer ${getCookie("token")}`,
        "Content-Type": "application/json",
      },
      // credentials: "same-origin"
    });
  } catch (error) {
    loadingLog.classList.add("d-none");
    e.target.classList.remove("d-none");
    validErrorsLog.innerHTML += `<li>Error</li>`;
    return;
  }
  if (response.ok) {
    response.text().then((data) => {
      document.cookie = "token=" + data + ";";
      window.location.href = "index.html";
    });
  } else {
    response.json().then((response) => {
      console.log(response[0]);
      for (i of response[0].errors) {
        validErrorsLog.innerHTML += `<li>${i.errorMessage}</li>`;
      }
      loadingLog.classList.add("d-none");
      e.target.classList.remove("d-none");
    });
  }

  // .then(response => {
  //     console.log(response);
  //     if (response.status == 200) {
  //         return response.text();//.json());
  //     }
  // })
  // .then(data => {
  //     console.log("Kek");
  //     // document.getElementById('result').innerText = "Added " + data;
  //     document.cookie = "token="+data+";";
  //     // window.location.href="index.html";
  // });
}
async function registration(e) {
  e.target.classList.add("d-none");
  loadingReg.classList.remove("d-none");
  validErrorsReg.innerHTML = "";
  if (pass.value != confirmpass.value) {
    return;
  }
  let user = {
    name: document.getElementById("userName").value,
    password: document.getElementById("pass").value,
    status: false,
  };

  let response;
  try {
    response = await fetch("http://localhost:38058/Auth/Register", {
      method: "POST",
      body: JSON.stringify(user),
      headers: { "Content-Type": "application/json" },
      // credentials: "same-origin"
    });
  } catch (error) {
    loadingReg.classList.add("d-none");
    e.target.classList.remove("d-none");
    validErrorsReg.innerHTML += `<li>Error</li>`;
    return;
  }

  if (response.ok) {
    response.text().then((data) => {
      document.cookie = "token=" + data + ";";
      window.location.href = "index.html";
    });
  } else {
    response.json().then((response) => {
      for (i of response[0].errors) {
        validErrorsReg.innerHTML += `<li>${i.errorMessage}</li>`;
      }
      loadingReg.classList.add("d-none");
      e.target.classList.remove("d-none");
    });
  }

  // .then(response => {
  //     console.log(response.json());
  //     console.log(response.statusText);
  //     if (response.status == 200) {
  //         return response.text();//.json());
  //     }
  // })
  // .then(data => {
  //     console.log(data);
  //     // document.getElementById('result').innerText = "Added " + data;
  //     document.cookie = "token="+data+";";
  //     console.log(data);
  //     // window.location.href="index.html";
  // });
  //         .then(response => response.json())
  // .then(response => console.log(response))
  // .catch(error => console.log(error));
}
