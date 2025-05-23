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

    // All selections now update the same #role input
    setupRoleSelection('#login_role_select_provider', '#login_role_select_employee', '#role');
    setupRoleSelection('#login_role_select_employee', '#login_role_select_provider', '#role');
    setupRoleSelection('#login_role_select_moderator', '#login_role_select_techadmin', '#role');
    setupRoleSelection('#login_role_select_techadmin', '#login_role_select_moderator', '#role');

    $("#switch-user-type").on("click", function () {
        const $providerSection = $("#role-select-provider");
        const $technicalSection = $("#role-select-technical");
        const $roleInput = $("#role");
        
        // Toggle visibility
        $providerSection.toggleClass("hidden");
        $technicalSection.toggleClass("hidden");
        
        if ($providerSection.hasClass("hidden")) {
            // Technical section is now visible, default to moderator
            $roleInput.val("moderator");

            $("#login_role_select_moderator").addClass("login-role-select--active");
            $("#login_role_select_techadmin").removeClass("login-role-select--active");
        } else {
            // Provider section is now visible, default to provider
            $roleInput.val("provider");

            $("#login_role_select_provider").addClass("login-role-select--active");
            $("#login_role_select_employee").removeClass("login-role-select--active");
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