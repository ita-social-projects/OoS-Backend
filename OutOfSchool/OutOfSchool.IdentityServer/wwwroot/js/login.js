let loginPasswordEye = document.getElementById('login_password_eye');
let loginPassword = document.getElementById('login_password');
let check_loginPasswordEye = false;

loginPasswordEye.addEventListener('click', function () {
    if (check_loginPasswordEye) {
        loginPasswordEye.src = "../icons/ic_eye.svg";
        check_loginPasswordEye = false;
        loginPassword.setAttribute("type", "Password");  
    } else {
        loginPasswordEye.src = "../icons/eye.svg";
        check_loginPasswordEye = true;
        loginPassword.setAttribute("type", "Text");  
    }
})