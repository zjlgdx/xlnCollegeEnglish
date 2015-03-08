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
            //if (lession == 2) {
            //    continue;
            //}

            for (var unit = 1; unit <= 8; unit++) {
                var textBook = '2';
                var count = 0;
                while (count < 2) {
                    var unitUrl = '~/horizonread' + lession + '/' + (unit + '').lpad('0', 2) + 'p' + textBook + 'newword1.htm';
                    pagelist.push(unitUrl);

                    if (textBook == "2") {
                        textBook = "3";
                    }
                    else {
                        textBook = "2";
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

        var wordTable = $(data).find('a.nw:first').closest('table');//.find('table');

        $(wordTable.get(0)).find('td').each(function (rowIndex) {
            var tdWord = $(this);
            var wordTag = tdWord.find('a.nw');
            var word = wordTag && $.trim(wordTag.text());
            if (word && word.length > 0) {
                var voice = wordTag.attr("onclick"); // todo: using regex to fetch extract file name
                voice = voice && voice.match(/(\')[^']*\1/);
                voice = voice && voice[0].replace(/\'/g, "");

                if (voice && voice.indexOf('http://') === 0) {
                    voice = voice.substring(voice.indexOf('sound'));
                }

                var paraphraseTag = tdWord.find('font[color="#336600"]');

                if (paraphraseTag && paraphraseTag.length > 0) {
                    paraphraseTag = $(paraphraseTag.get(0));
                }

                var paraphrase = $.trim(paraphraseTag.text());// todo : remove new line "\n"
                var paraphraseVoice = paraphraseTag.find('img').attr("onclick");// todo: using regex to fetch extract file name
                paraphraseVoice = paraphraseVoice && paraphraseVoice.match(/(\')[^']*\1/);
                paraphraseVoice = paraphraseVoice && paraphraseVoice[0].replace(/\'/g, "");

                if (paraphraseVoice && paraphraseVoice.indexOf('http://') === 0) {
                    paraphraseVoice = paraphraseVoice.substring(paraphraseVoice.indexOf('sound'));
                }

                var wholeText = $.trim($.trim(tdWord.text()).replace(/^\d+\./, ""));
                var pattern1 = new RegExp("^" + escape(word));
                var pattern2 = new RegExp(escape(paraphrase) + "$");
                var definition = $.trim(escape(wholeText).replace(pattern1, "").replace(pattern2, ""));// todo : remove new line "\n"
                definition = definition && unescape(definition)

                paraphrase = paraphrase.replace(/[\n\r]/g, "").replace(/\s{10,}/g, " ");
                definition = definition.replace(/[\n\r]/g, "").replace(/\s{10,}/g, " ");
                word = word.replace(/[\n\r]/g, "").replace(/\s{10,}/g, " ");
                var vocabulary = new Vocabulary(word, voice, definition, paraphrase, paraphraseVoice);
                unit.push(vocabulary);
            }


            //<font color="#336600"><span title="Audio"><img src="images/icon_audio_small.gif" tppabs="http://educenter.fudan.edu.cn/collegeenglish/new/integrated1/images/icon_audio_small.gif" width="16" height="10" border="0" onclick="MediaPlayer.Open('sound/vua01.mp3'/*tpa=http://educenter.fudan.edu.cn/collegeenglish/new/integrated1/sound/vua01.mp3*/);" class="hand"></span> 
            //&nbsp;&nbsp;e.g.  It has been raining off and on for a week.</font>
        });


        //#000000
        var properName = $(data).find('font[color="#000000"]');
        properName.each(function () {
            var pn = $(this);
            if ($.trim(pn.text()) === "Proper Names") {
                var trPn = pn.closest('tr');
                var trPnSiblings = trPn.siblings();

                //var properNamesTable = $(trPnSiblings).find('a.nw').closest('table');//.find('table');
                getproperNames(trPn, unit)
                getproperNames(trPnSiblings, unit)
                //$(trPnSiblings).find('font[color="#3366cc"]').each(function () {
                //    var pnWordTag = $(this);
                //    var enTag = pnWordTag.find('a.nw');
                //    var en = $.trim(enTag.text())
                //    var enCn = $.trim($.trim(pnWordTag.text()).replace(/^\d+\./, ""))
                //    var pattern1 = new RegExp("^" + en);
                //    var cn = $.trim(enCn.replace(pattern1, ""))

                //    var envoice = enTag.attr("onclick"); // todo: using regex to fetch extract file name
                //    envoice = envoice && envoice.match(/(\')[^']*\1/);
                //    envoice = envoice && envoice[0].replace(/\'/g, "");

                //    en = en.replace(/[\n\r]/g, "").replace(/\s{15,}/g, "");
                //    cn = cn.replace(/[\n\r]/g, "").replace(/\s{15,}/g, "");

                //    var voc = new Vocabulary(en, envoice, cn);
                //    unit.push(voc);
                //});
            }
        });
        return unit;

    }

    function getproperNames(trs, unit)
    {
        var newpn = [];
        $(trs).find('font[color="#3366cc"]').each(function () {
            var pnWordTag = $(this);
            //var enTag = pnWordTag.find('a.nw');
            //var en = $.trim(enTag.text())
            var enCn = $.trim($.trim(pnWordTag.text()).replace(/^\d+\./, ""))
            //var pattern1 = new RegExp("^" + en);
            //var cn = $.trim(enCn.replace(pattern1, ""))

            //var envoice = enTag.attr("onclick"); // todo: using regex to fetch extract file name
            //envoice = envoice && envoice.match(/(\')[^']*\1/);
            //envoice = envoice && envoice[0].replace(/\'/g, "");

            //en = en.replace(/[\n\r]/g, "").replace(/\s{10,}/g, " ");
            //cn = cn.replace(/[\n\r]/g, "").replace(/\s{10,}/g, " ");
            enCn = enCn && enCn.replace(/[\n\r]/g, "").replace(/\s{10,}/g, " ").replace(/\t/g, "");
            //if (cn && cn.indexOf('O.Henry') != -1) {
            //    var ccccc = cn;
            //}

            if (enCn && enCn.length > 0) {
                var voc = new Vocabulary(enCn);
                newpn.push(voc);
            }
            
        });
        var index =0;
        $(trs).find('a.nw').each(function () {
            var pnWordTag = $(this);
            //var enTag = pnWordTag.find('a.nw');
            //var en = $.trim(enTag.text())
            //var enCn = $.trim($.trim(pnWordTag.text()).replace(/^\d+\./, ""))
            //var pattern1 = new RegExp("^" + en);
            //var cn = $.trim(enCn.replace(pattern1, ""))

            var envoice = pnWordTag.attr("onclick"); // todo: using regex to fetch extract file name
            envoice = envoice && envoice.match(/(\')[^']*\1/);
            envoice = envoice && envoice[0].replace(/\'/g, "");
            if (envoice && envoice.indexOf('http://') === 0) {
                envoice = envoice.substring(envoice.indexOf('sound'));
            }
            //en = en.replace(/[\n\r]/g, "").replace(/\s{10,}/g, " ");
            //cn = cn.replace(/[\n\r]/g, "").replace(/\s{10,}/g, " ");
            //enCn = enCn && enCn.replace(/[\n\r]/g, "").replace(/\s{10,}/g, " ");
            //if (cn && cn.indexOf('O.Henry') != -1) {
            //    var ccccc = cn;
            //}
            newpn[index++].Voice = envoice;
           
               

        });

        for (var i = 0; i < newpn.length; i++) {
            unit.push(newpn[i]);
        }
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

function Vocabulary(word, voice, definition, paraphrase, paraphraseVoice) {
    this.Word = word;
    this.Voice = voice;
    this.Definition = definition;
    this.Paraphrase = paraphrase;
    this.ParaphraseVoice = paraphraseVoice;
}

//function Definition(partofspeech, paraphrase) {
//    this.PartofSpeech = partofspeech;
//    this.Paraphrase = paraphrase;
//}