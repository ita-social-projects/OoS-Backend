let btn_parent = document.getElementById('btn_parent');
let btn_provider = document.getElementById('btn_provider');
let btn_register = document.getElementById('btn_register');

let passwordEye = document.getElementById('password_eye');
let confirmPasswordEye = document.getElementById('confirm_password_eye');

let check_passwordEye = false;
let check_confirmPasswordEye = false;

let password = document.getElementById('password');
let repeatPassword = document.getElementById('repeat_password');

btn_parent.addEventListener('click', function () {
    btn_parent.className = "registration_type registration_type-active";
    btn_provider.className = "registration_type";
    btn_register.setAttribute("name", "Parent"); 
})

btn_provider.addEventListener('click', function () {
    btn_provider.className = "registration_type registration_type-active";
    btn_parent.className = "registration_type";
    btn_register.setAttribute("name", "Provider");  
})

passwordEye.addEventListener('click', function () {
    if (check_passwordEye) {
        passwordEye.src = "/icons/ic_eye.svg";
        check_passwordEye = false;
        password.setAttribute("type", "Password");  
    } else {
        passwordEye.src = "/icons/eye.svg";
        check_passwordEye = true;
        password.setAttribute("type", "Text");  
    }  
})

confirmPasswordEye.addEventListener('click', function () {
    if (check_confirmPasswordEye) {
        confirmPasswordEye.src = "/icons/ic_eye.svg";
        check_confirmPasswordEye = false;
        repeatPassword.setAttribute("type", "Password");  
    } else {
        confirmPasswordEye.src = "/icons/eye.svg";
        check_confirmPasswordEye = true;
        repeatPassword.setAttribute("type", "Text");  
    }
})