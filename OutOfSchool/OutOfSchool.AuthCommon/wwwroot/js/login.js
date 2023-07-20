$(function() {
    let check_loginPasswordEye = false;
    const $loginPassword = $("#login_password");

    $("#login_password_eye").on("click", function () {
        if (check_loginPasswordEye) {
            $(this).attr("src", "../_content/auth/icons/ic_eye.svg");
            check_loginPasswordEye = false;
            $loginPassword.attr("type", "Password");
        } else {
            $(this).attr("src", "../_content/auth/icons/eye.svg");
            check_loginPasswordEye = true;
            $loginPassword.attr("type", "Text");
        }
    });
});