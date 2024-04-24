$.validator.addMethod("validpass",
    function (value, element, params) {
        const valid = params.validations.reduce((acc, next) => {
            const check = Array.from(value).some(next);
            return check && acc;
        }, true)
        return valid && value.length >= params.minlength;
    });
$.validator.unobtrusive.adapters.add("validpass", ['minlength', 'symbols', 'upper', 'lower', 'number'], function (options) {
    const rules = [];
    if (options.params.upper) {
        rules.push((c) => c === c.toUpperCase())
    }
    if (options.params.lower) {
        rules.push((c) => c === c.toLowerCase())
    }
    if (options.params.number) {
        rules.push((c) => !isNaN(c))
    }

    if (options.params.symbols) {
        rules.push((c) => options.params.symbols.includes(c))
    }
    options.rules["validpass"] = {minlength: options.params.minlength, validations: rules};
    options.messages["validpass"] = options.message;
});