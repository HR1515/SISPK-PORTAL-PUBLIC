var NewFormValidation = function () {

    // validation using icons
    var handleValidation2deni = function () {
        // for more info visit the official plugin documentation: 
        // http://docs.jquery.com/Plugins/Validation
        var ini = $(this);
        var form2 = $('#sispk_form');
        var error2 = $('.alert-danger', form2);
        var success2 = $('.alert-success', form2);

        form2.validate({
            errorElement: 'span', //default input error message container
            errorClass: 'help-block help-block-error', // default input error message class
            focusInvalid: false, // do not focus the last invalid input
            ignore: ':hidden',
            rules: {
                name: {
                    minlength: 2,
                    required: true
                },
                email: {
                    required: true,
                    email: true
                },
                email: {
                    required: true,
                    email: true
                },
                url: {
                    required: true,
                    url: true
                },
                number: {
                    required: true,
                    number: true
                },
                digits: {
                    required: true,
                    digits: true
                },
                creditcard: {
                    required: true,
                    creditcard: true
                },
            },

            invalidHandler: function (event, validator) { //display error alert on form submit              
                success2.hide();
                error2.show();
                Layout.scrollTo(error2, -200);
            },

            errorPlacement: function (error, element) { // render error placement for each input type
                var icon = $(element).parent('.input-icon').children('i');
                icon.removeClass('fa-check').addClass("fa-warning");
                icon.attr("data-original-title", error.text()).tooltip({ 'container': 'body' });
                if (element.parent(".input-group").size() > 0) {
                    error.insertAfter(element.parent(".input-group"));
                } else if (element.attr("data-error-container")) {
                    error.appendTo(element.attr("data-error-container"));
                } else if (element.parents('.radio-list').size() > 0) {
                    error.appendTo(element.parents('.radio-list').attr("data-error-container"));
                } else if (element.parents('.radio-inline').size() > 0) {
                    error.appendTo(element.parents('.radio-inline').attr("data-error-container"));
                } else if (element.parents('.checkbox-list').size() > 0) {
                    error.appendTo(element.parents('.checkbox-list').attr("data-error-container"));
                } else if (element.parents('.checkbox-inline').size() > 0) {
                    error.appendTo(element.parents('.checkbox-inline').attr("data-error-container"));
                } else {
                    error.insertAfter(element); // for other inputs, just perform default behavior
                }
            },
            //highlight: function (element) { // hightlight error inputs
            //    $(element)
            //        .closest('.form-group').removeClass("has-success").addClass('has-error'); // set error class to the control group   
            //},

            //unhighlight: function (element) { // revert the change done by hightlight

            //},
            highlight: function (element) { // hightlight error inputs
                $(element)
                     .closest('.form-group').addClass('has-error').addClass('deni-error'); // set error class to the control group
            },

            unhighlight: function (element) { // revert the change done by hightlight
                $(element)
                    .closest('.form-group').removeClass('has-error'); // set error class to the control group
            },

            //success: function (label) {
            //    label
            //        .closest('.form-group').removeClass('has-error'); // set success class to the control group
            //},
            success: function (label, element) {
                var icon = $(element).parent('.input-icon').children('i');
                $(element).closest('.form-group').removeClass('has-error').addClass('has-success'); // set success class to the control group
                icon.removeClass("fa-warning").addClass("fa-check");
                label.closest('.form-group').removeClass('has-error'); // set success class to the control group
            },

            submitHandler: function (form) {
                success2.show();
                error2.hide();
                $('#konfirmasipenyimpanan').modal('show');
                $('#setujukonfirmasibutton').on('click', function () {
                    //bukaloading();
                    form.submit();
                });
                //form.submit(); // submit the form
            }
        });
        jQuery('.select2ajax', form2).change(function () {
            form2.validate().element($(this)); //revalidate the chosen dropdown value and show error or success message for the input
        });
        jQuery('.select2me', form2).change(function () {
            form2.validate().element($(this)); //revalidate the chosen dropdown value and show error or success message for the input
        });
        jQuery.validator.addClassRules({
            wajib: {
                required: true
            },
            wajibcheckbox: {
                required: true
            },
            wajibfile: {
                required: true,
                extension: "xls|csv"
            }
        });

        
        jQuery.extend(jQuery.validator.messages, {
            required: "Kolom ini Wajib di isi.",
            remote: "Please fix this field.",
            email: "Harap Gunakan Format Email.",
            url: "Please enter a valid URL.",
            date: "Harap Gunakan Format Tanggal",
            dateISO: "Please enter a valid date (ISO).",
            number: "Hanya Angka yang di perbolehkan",
            digits: "Hanya Angka yang di perbolehkan",
            creditcard: "Please enter a valid credit card number.",
            equalTo: "Please enter the same value again.",
            accept: "Please enter a value with a valid extension.",
            maxlength: jQuery.validator.format("Kurang dari {0} characters."),
            minlength: jQuery.validator.format("Lebih {0} characters."),
            rangelength: jQuery.validator.format("Please enter a value between {0} and {1} characters long."),
            range: jQuery.validator.format("Please enter a value between {0} and {1}."),
            max: jQuery.validator.format("Maksimal {0}."),
            min: jQuery.validator.format("Minimal {0}."),
        });
    

    }

    var handleWysihtml5 = function () {
        if (!jQuery().wysihtml5) {
            return;
        }

        if ($('.wysihtml5').size() > 0) {
            $('.wysihtml5').wysihtml5({
                "stylesheets": ["../../assets/global/plugins/bootstrap-wysihtml5/wysiwyg-color.css"]
            });
        }
    }

    return {
        //main function to initiate the module
        init: function () {
            handleValidation2deni();
            handleWysihtml5();
            

        }

    };

}();