// From Registrasi
if ($('#frmRegistrasi').length > 0) {
    //Initialize
    $('#kotakab').prop('disabled', true);


    $('#check_agreement').click(function () {
        $('#responsive-agreement').css("z-index", "99999");
        $('#responsive-agreement').modal();
    });
    
    //$('.select2').select2({
    //    allowClear: true
    //});

    $('#provinsi').select2({ allowClear: true });

    $('#provinsi').change(function () {
        alert();
        $("#kotakab").select2().val(null).trigger("change");
        if ($(this).val() != 0) {
            $('#kotakab').prop('disabled', false);
            var idParent = $('#provinsi').val();
            var opt = "<option>Pilih Kabupaten</option>";
            $.ajax({
                type: 'POST',
                url: MainUrl + 'main/GetKotaKab/' + idParent,
                data: { id: idParent },
                cache: false,
                dataType: 'json',
                success: function (data) {
                    $('#kotakab').empty();
                    opt += data.value;
                    $('#kotakab').addClass("input-large");
                    $('#kotakab').addClass("select2");
                    $('#kotakab').html("").append(opt);
                }
            });
        } else {
            $('#kotakab').val('');
            $('#kotakab').empty();
            $('#kotakab').removeClass("input-large");
            $('#kotakab').removeClass("select2");
            $("#kotakab").prop("disabled", true);
        }
    });
}