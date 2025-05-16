$(function () {
    function setupRoleSelection(selector, oppositeSelector, inputId) {
        $(selector).on('click', function () {
            $(oppositeSelector).removeClass('login-role-select--active');
            $(this).addClass('login-role-select--active');

            // Update the hidden input with the selected value
            let selectedValue = $(this).data('value');
            $(inputId).val(selectedValue);
        });
    }

    // Setup role selections
    setupRoleSelection('#login_role_select_provider', '#login_role_select_employee', '#role');
    setupRoleSelection('#login_role_select_employee', '#login_role_select_provider', '#role');
    setupRoleSelection('#login_role_select_moderator', '#login_role_select_techadmin', '#technical-staff-role');
    setupRoleSelection('#login_role_select_techadmin', '#login_role_select_moderator', '#technical-staff-role');

    $("#switch-user-type").on("click", function () {
        $("#for_technical_staff, #for_providers").toggleClass("hidden");
    });

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