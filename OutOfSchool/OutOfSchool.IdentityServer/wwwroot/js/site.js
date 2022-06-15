let btn_parent = document.getElementById('btn_parent');
let btn_provider = document.getElementById('btn_provider');
let btn_register = document.getElementById('btn_register');

let passwordEye = document.getElementById('password_eye');
let confirmPasswordEye = document.getElementById('confirm_password_eye');

let check_passwordEye = false;
let check_confirmPasswordEye = false;

let password = document.getElementById('password');
let repeatPassword = document.getElementById('repeat_password');

let ageConfirm = document.getElementById('checkbox_age_confirm');
let rulesAgreement = document.getElementById('checkbox_rules_agreement');

btn_register.disabled = true;

if (sessionStorage.getItem("Button") && sessionStorage.getItem("Role")) {
    btn_provider.className = "registration_type";
    btn_parent.className = "registration_type";
    btn_register.setAttribute("name", sessionStorage.getItem("Role"));
    document.getElementById(sessionStorage.getItem("Button")).className = "registration_type registration_type-active";
}

if (password.className.includes('input-validation-error')) {
    let elements = document.getElementsByClassName('registration_privacy_password');
    for (let element of elements) {
        element.style.height = "105px";
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
        passwordEye.src = "../icons/ic_eye.svg";
        check_passwordEye = false;
        password.style.fontSize = "20px";
        password.setAttribute("type", "Password");
       
    } else {
        passwordEye.src = "../icons/eye.svg";
        check_passwordEye = true;
        password.style.fontSize = "20px";
        password.setAttribute("type", "Text");  
    }  
})

confirmPasswordEye.addEventListener('click', function () {
    if (check_confirmPasswordEye) {
        confirmPasswordEye.src = "../icons/ic_eye.svg";
        check_confirmPasswordEye = false;
        repeatPassword.style.fontSize = "20px";
        repeatPassword.setAttribute("type", "Password");  
    } else {
        confirmPasswordEye.src = "../icons/eye.svg";
        check_confirmPasswordEye = true;
        repeatPassword.style.fontSize = "20px";
        repeatPassword.setAttribute("type", "Text");  
    }
});

function validateFormMaturity(form) {
    if (!form.maturity.checked) {
        document.getElementById('maturity').style.visibility = 'visible';
        return false;
    }
    else {
        document.getElementById('maturity').style.visibility = 'hidden';
        return true;
    }    
}

function validateFormAccept(form) {
    if (!form.accept.checked) {
        document.getElementById('accept').style.visibility = 'visible';
        return false;
    } else {
        document.getElementById('accept').style.visibility = 'hidden';
        return true;
    }
}

function validateForm(form) {
    let isValidMature = validateFormMaturity(form);
    let isValidAccept = validateFormAccept(form);

    return (isValidMature && isValidAccept) ? true : false;
}

function validateFormOnEvent(form) {
    let valid = allFieldsValid(form);

    if (btn_register.disabled === valid)
        btn_register.disabled = !valid;
}

function allFieldsValid(form) {
    if(!ageConfirm.checked || !rulesAgreement.checked){
        return false;
    }

    let registrationInputs = form.getElementsByClassName("registration_input_required");
    for (var i = 0; i < registrationInputs.length; i++) {
        if (registrationInputs.item(i).value === '')
            return false;
    }
    return true;
}


