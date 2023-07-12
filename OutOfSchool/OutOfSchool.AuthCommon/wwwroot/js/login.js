let loginPasswordEye = document.getElementById('login_password_eye');
let loginPassword = document.getElementById('login_password');
let check_loginPasswordEye = false;

loginPasswordEye.addEventListener('click', function () {
    if (check_loginPasswordEye) {
        loginPasswordEye.src = "../_content/auth/icons/ic_eye.svg";
        check_loginPasswordEye = false;
        loginPassword.setAttribute("type", "Password");  
    } else {
        loginPasswordEye.src = "../_content/auth/icons/eye.svg";
        check_loginPasswordEye = true;
        loginPassword.setAttribute("type", "Text");  
    }
})