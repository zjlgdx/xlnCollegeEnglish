$(function () {
    $('button').click(function () {
        var pagelist = getPageList();

        for (var i = 0; i < pagelist.length; i++) {
            var pageUrl = pagelist[i];
            getUnitPage(pageUrl);

        }
    });


    function getPageList() {
        var pagelist = [];

        for (var lession = 1; lession <= 4; lession++) {
            // can not access lession 2
            if (lession == 2) {
                continue;
            }

            for (var unit = 1; unit <= 10; unit++) {
                var textBook = 'A';
                var count = 0;
                while (count < 2) {
                    var unitUrl = '~/horizonread' + lession + '/UNIT' + (unit + '').lpad('0', 2) + '-' + textBook + '-lw.htm';
                    pagelist.push(unitUrl);

                    if (textBook == "A") {
                        textBook = "B";
                    }
                    else {
                        textBook = "A";
                    }

                    count++;
                }

            }
        }

        return pagelist;
    }

    function getUnitPage(pageUrl) {
        $("#output").append('<li style="color:green;">getting page: ' + pageUrl + ' content.</li>');
        var jqxhr = $.get("/GetUnitPageHandler.ashx", { url: pageUrl }, function (data) {
            $("#output").append('<li style="color:green;">getted page: ' + pageUrl + ' content success.</li>');
            // return data;
        }).fail(function (xhr, ajaxOptions, thrownError) {
            $("#output").append('<li style="color:red;">' + xhr.responseText + '</li>');
        });

        jqxhr.done(function (data) {
            var pageUrlId = pageUrl.replace(/[\~\.\/\-]/g, '_');
            $('#UnitListContent').append('<div id="' + pageUrlId + '">').append(data)
            var unitWords = getUnitWords(data);
            var unit = new Unit(pageUrlId);
            unit.Vocabularies = unitWords
            var jsonData = JSON.stringify(unit);

            saveUnitWords(jsonData, pageUrl);
        });

        return jqxhr;
    }

    function getUnitWords(data) {
        var unit = [];
        //$(data).find('#nw-area')
        $($(data).get(0)).find('table').each(function (index) {
            console.log('table' + index)
            var wordTable = $(this).find('table:even');
            var word = wordTable.find('td[bgcolor="#6699CC"]').text();
            var voice = wordTable.find('td img[id="playsw"]').attr('sound');
            if (word != "") {

                var vocabulary = new Vocabulary(word, voice);
                console.log('word:' + word)
                console.log('voice:' + voice)

                var definitionTable = $(this).find('table:odd');

                definitionTable.find('tr').each(function () {
                    var partofSpeech = $(this).find('td:even').text();
                    var paraphrase = $(this).find('td:odd').text();

                    var definition = new Definition(partofSpeech, paraphrase);
                    vocabulary.Definitions.push(definition);
                    console.log('partofSpeech:' + partofSpeech)
                    console.log('paraphrase:' + paraphrase)
                });

                unit.push(vocabulary);
            }
        });

        return unit;
    }

    function saveUnitWords(jsonData, pageUrl) {
       
        $("#output").append('<li style="color:green;">saving ' + pageUrl + '</li>');
        var jqxhr = $.ajax({
            url: "/UnitHandler.ashx", //make sure the path is correct
            cache: false,
            type: 'POST',
            data: jsonData,
            success: function (response) {
                //output the response from server
                $("#output").append('<li style="color:green;">saved ' + pageUrl + ' ' + response + '</li>');
            },
            error: function (xhr, ajaxOptions, thrownError) {
                $("#output").append('<li style="color:red;">' + xhr.responseText + '</li>');
            }
        });

    }
})

// http://sajjadhossain.com/2008/10/31/javascript-string-trimming-and-padding/
//pads left
String.prototype.lpad = function (padString, length) {
    var str = this;
    while (str.length < length)
        str = padString + str;
    return str;
}

//pads right
String.prototype.rpad = function (padString, length) {
    var str = this;
    while (str.length < length)
        str = str + padString;
    return str;
}

function Unit(unitTitle) {
    this.UnitTitle = unitTitle;
    this.Vocabularies = [];
}

function Vocabulary(word, voice) {
    this.Word = word;
    this.Voice = voice;
    this.Definitions = [];
}

function Definition(partofspeech, paraphrase) {
    this.PartofSpeech = partofspeech;
    this.Paraphrase = paraphrase;
}