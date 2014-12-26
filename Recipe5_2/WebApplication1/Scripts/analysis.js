$(function () {
    $('button').click(function () {
        var tables = $('table').find('td').each(function(){
            console.log($.trim($(this).text()));
            
        });
        

    });
})