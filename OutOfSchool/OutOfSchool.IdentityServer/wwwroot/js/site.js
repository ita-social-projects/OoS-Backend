﻿let btn_parent = document.getElementById('btn_parent');
let btn_provider = document.getElementById('btn_provider');
let btn_register = document.getElementById('btn_register');

let passwordEye = document.getElementById('password_eye');
let confirmPasswordEye = document.getElementById('confirm_password_eye');

let check_passwordEye = false;
let check_confirmPasswordEye = false;

let password = document.getElementById('password');
let repeatPassword = document.getElementById('repeat_password');

if (sessionStorage.getItem("Button") && sessionStorage.getItem("Role")) {
    btn_provider.className = "registration_type";
    btn_parent.className = "registration_type";
    btn_register.setAttribute("name", sessionStorage.getItem("Role"));
    document.getElementById(sessionStorage.getItem("Button")).className = "registration_type registration_type-active";
}

if (password.className.includes('input-validation-error')) {
    let elements = document.getElementsByClassName('registration_privacy_password');
    for (let element of elements) {
        element.style.height = "65px";
    } 
}

if (repeatPassword.className.includes('input-validation-error')) {
    let elements = document.getElementsByClassName('registration_privacy_confirm_password');
    for (let element of elements) {
        element.style.height = "65px";
    }
}

if (document.getElementsByClassName('validation-summary-errors').length > 0) {
    document.getElementById("user_mail").className += ' input-validation-error';
}

btn_parent.addEventListener('click', function () {
    btn_parent.className = "registration_type registration_type-active";
    btn_provider.className = "registration_type";
    btn_register.setAttribute("name", "Parent");
    sessionStorage.setItem("Role", "Parent");
    sessionStorage.setItem("Button", "btn_parent");
})

btn_provider.addEventListener('click', function () {
    btn_provider.className = "registration_type registration_type-active";
    btn_parent.className = "registration_type";
    btn_register.setAttribute("name", "Provider");
    sessionStorage.setItem("Role", "Provider");
    sessionStorage.setItem("Button", "btn_provider");
})

passwordEye.addEventListener('click', function () {
    if (check_passwordEye) {
        passwordEye.src = "/icons/ic_eye.svg";
        check_passwordEye = false;
        password.style.fontSize = "30px";
        password.setAttribute("type", "Password");
       
    } else {
        passwordEye.src = "/icons/eye.svg";
        check_passwordEye = true;
        password.style.fontSize = "20px";
        password.setAttribute("type", "Text");  
    }  
})

confirmPasswordEye.addEventListener('click', function () {
    if (check_confirmPasswordEye) {
        confirmPasswordEye.src = "/icons/ic_eye.svg";
        check_confirmPasswordEye = false;
        repeatPassword.style.fontSize = "30px";
        repeatPassword.setAttribute("type", "Password");  
    } else {
        confirmPasswordEye.src = "/icons/eye.svg";
        check_confirmPasswordEye = true;
        repeatPassword.style.fontSize = "20px";
        repeatPassword.setAttribute("type", "Text");  
    }
})

function validateForm(form) {
    if (!form.maturity.checked) {
        document.getElementById('maturity').style.visibility = 'visible';
        return false;
    }
    else {
        document.getElementById('maturity').style.visibility = 'hidden';
        return true;
    }
}



