$(function () {
    $('#login_role_select_provider').on('click', function () {
        $('#login_role_select_employee').removeClass('login-role-select--active');
        $(this).addClass('login-role-select--active');

        // Update the hidden input with the selected value
        let selectedValue = $(this).data('value');
        $('#role').val(selectedValue);
    });

    $('#login_role_select_employee').on('click', function () {
        $('#login_role_select_provider').removeClass('login-role-select--active');
        $(this).addClass('login-role-select--active');

        // Update the hidden input with the selected value
        let selectedValue = $(this).data('value');
        $('#role').val(selectedValue);
    });

    $('#login_role_select_moderator').on('click', function () {
        $('#login_role_select_techadmin').removeClass('login-role-select--active');
        $(this).addClass('login-role-select--active');

        // Update the hidden input with the selected value
        let selectedValue = $(this).data('value');
        $('#technical-staff-role').val(selectedValue);
    });

    $('#login_role_select_techadmin').on('click', function () {
        $('#login_role_select_moderator').removeClass('login-role-select--active');
        $(this).addClass('login-role-select--active');

        // Update the hidden input with the selected value
        let selectedValue = $(this).data('value');
        $('#technical-staff-role').val(selectedValue);
    });

    $("#switch-user-type").on("click", function () {
        var x = document.getElementById("for_technical_staff");
        var y = document.getElementById("for_providers");

        if (x.style.display === "none") {
            x.style.display = "block";
            y.style.display = "none";
        } else {
            x.style.display = "none";
            y.style.display = "block";
        }
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