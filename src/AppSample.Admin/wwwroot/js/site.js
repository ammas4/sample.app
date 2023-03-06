// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function tabClick(index) {
    document.getElementById('SelectedTab').value = index;
}

function addAuth() {
    $('#authenticatorEditor').append('<li class="sort-auth-item">\n' +
        '                    <div class="auth">\n' +
        '                        <div class="row">\n' +
        '                            <ul class="sort-channel">\n' +
        '                            </ul>\n' +
        '                            \n' +
        '                            <a href="javascript:void(0)" onclick="addAuthChannel(this)">Добавить аутентификатор</a>\n' +
        '                        </div>\n' +
        '                    </div>\n' +
        '                </li>')
}

function addAuthChannel(obj) {
    $.get('/ServiceProviders/AuthenticatorEntryRow', function (template) {
        $(obj)
            .parent()
            .find('.sort-channel')
            .append(template);

        recalculateLevel1();
        recalculateLevel2();
    });
}

$('#authenticatorEditor').on('click', '.pparent-remove', function () {
    $(this).parent().parent().parent().remove();
});

function recalculateLevel1(){
    $('.sort-channel').each(function(i, obj) {
        $('*[id$=OrderLevel1]', obj).each(function(j, obj2) {
            obj2.value = i;
        });
    });
}

function recalculateLevel2() {
    $('.sort-channel').each(function(i, obj) {
        $('*[id$=OrderLevel2]', obj).each(function(j, obj2) {
            obj2.value = j;
        });
    });
}

$(function() {
    $(".sort-auth").sortable();
    $(".sort-auth").disableSelection();
    $(".sort-auth").sortable({
        axis: "y"
    });

    $(".sort-auth").sortable({
        deactivate: function(event, ui) {
            recalculateLevel1();
        }
    });
});

$(function() {
    $(".sort-channel").sortable();
    $(".sort-channel").disableSelection();
    $(".sort-channel").sortable({
        axis: "y"
    });

    $(".sort-channel").sortable({
        deactivate: function(event, ui) {
            recalculateLevel2();
        }
    });
});