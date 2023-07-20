$(function () {
    function setPersonalLinksAndAttributesVisibility(button) {
        if (button === "btn_provider") {
            $("#li_date_of_birth").hide();
            $("#li_gender").hide();
            $("#link_privacy_terms").attr("href", "/Privacy/ProviderTerms");
        } else {
            $("#li_date_of_birth").show();
            $("#li_gender").show();
            $("#link_privacy_terms").attr("href", "/Privacy/ParentTerms");
        }
    }

    let check_passwordEye = false;
    let check_confirmPasswordEye = false;

    const $buttonProvider = $("#btn_provider");
    const $buttonParent = $("#btn_parent");
    const $buttonRegister = $("#btn_register");
    const $password = $("#password");
    const $repeatPassword = $("#repeat_password");

    $buttonRegister.prop("disabled", true);
    setPersonalLinksAndAttributesVisibility(sessionStorage.getItem("Button"));

    if (sessionStorage.getItem("Button") && sessionStorage.getItem("Role")) {
        $buttonProvider.removeClass("registration_type-active");
        $buttonParent.removeClass("registration_type-active");
        $buttonRegister.attr("name", sessionStorage.getItem("Role"));
        $(`#${sessionStorage.getItem("Button")}`).addClass("registration_type-active");
        setPersonalLinksAndAttributesVisibility(sessionStorage.getItem("Button"));
    }

    if ($password.hasClass("input-validation-error")) {
        $(".registration_privacy_password").css("height", "105px");
    }

    if ($repeatPassword.hasClass('input-validation-error')) {
        $(".registration_privacy_confirm_password").css("height", "65px");
    }

    if ($(".validation-summary-errors").length) {
        $("#user_mail").addClass("input-validation-error");
    }

    $buttonParent.on('click', function () {
        $buttonParent.addClass("registration_type-active");
        $buttonProvider.removeClass("registration_type-active");
        $buttonRegister.attr("name", "Parent");
        sessionStorage.setItem("Role", "Parent");
        sessionStorage.setItem("Button", "btn_parent");
        setPersonalLinksAndAttributesVisibility(sessionStorage.getItem("Button"));
    })

    $buttonProvider.on('click', function () {
        $buttonProvider.addClass("registration_type-active");
        $buttonParent.removeClass("registration_type-active");
        $buttonRegister.attr("name", "Provider");
        sessionStorage.setItem("Role", "Provider");
        sessionStorage.setItem("Button", "btn_provider");
        setPersonalLinksAndAttributesVisibility(sessionStorage.getItem("Button"));
    })

    $("#password_eye").on('click', function () {
        if (check_passwordEye) {
            $(this).attr("src", "../_content/auth/icons/ic_eye.svg");
            check_passwordEye = false;
            $password.css("fontSize", "20px");
            $password.attr("type", "Password");

        } else {
            $(this).attr("src", "../_content/auth/icons/eye.svg");
            check_passwordEye = true;
            $password.css("fontSize", "20px");
            $password.attr("type", "Text");
        }
    })
    $("#confirm_password_eye").on('click', function () {
        if (check_confirmPasswordEye) {
            $(this).attr("src", "../_content/auth/icons/ic_eye.svg");
            check_confirmPasswordEye = false;
            $repeatPassword.css("fontSize", "20px");
            $repeatPassword.attr("type", "Password");
        } else {
            $(this).attr("src", "../_content/auth/icons/eye.svg");
            check_confirmPasswordEye = true;
            $repeatPassword.css("fontSize", "20px");
            $repeatPassword.attr("type", "Text");
        }
    });
});

function validateFormMaturity(form) {
    if (!form.maturity.checked) {
        $("#maturity").css("visibility", "visible");
        return false;
    }
    else {
        $("#maturity").css("visibility", "hidden");
        return true;
    }    
}

function validateFormAccept(form) {
    if (!form.accept.checked) {
        $("#accept").css("visibility", "visible");
        return false;
    } else {
        $("#accept").css("visibility", "hidden");
        return true;
    }
}

function validateForm(form) {
    let isValidMature = validateFormMaturity(form);
    let isValidAccept = validateFormAccept(form);

    return isValidMature && isValidAccept;
}

function validateFormOnEvent(form) {
    let valid = allFieldsValid(form);
    $("#btn_register").prop("disabled", !valid)
}

function allFieldsValid(form) {
    if(!$("#checkbox_age_confirm").checked() || !$("#checkbox_rules_agreement").checked()){
        return false;
    }

    let registrationInputs = form.getElementsByClassName("registration_input_required");
    for (var i = 0; i < registrationInputs.length; i++) {
        if (registrationInputs.item(i).value === '')
            return false;
    }
    return true;
}
