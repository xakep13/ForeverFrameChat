var Iframe = new function () {
    var self = this;
    var connectTries = 0,
        reconnectTimer;
    var htmlfile; // for ie only
    var iframe;

    this.onConnected = function () {
        connectTries = 0;
        clearTimeout(reconnectTimer);
    };

    this.onMessage = function (message) {
        var data = window.JSON.parse(message);
        switch (data.Type) {
            case 0:
                SendPrivateMessage(data.Id, data.UserName, data.Value, false);
                break;
            case 1:
                 AddMessage(data.UserName, data.Value);
                break;
            case 2:
                if (data.Id != $('#hdId').val()) {
                    AddUser(data.Id, data.UserName);
                    AddMessage(data.UserName, data.Value);
                }
                break;
            case 3:
                AddMessage(data.UserName, data.Value);
                RemoveUser(data.Id, data.UserName);
                break;
            case 4:
                JoinUser(data.Id);
                JoimMessage(data.Value);
                AddMessage(data.UserName, "Приєднався");
                break;
            case 5:
                if (data.Id != $('#hdId').val())
                    SayWhoIsTyping(data.UserName);
                break;
            case 6:
                
                    SendPrivateMessage(data.Id, data.UserName, data.Value, true);
                break;
            case 7:
                SendGroupMessage(data.Id, data.UserName, data.Value, false);
                break;
            case 8:
                if (data.Id != $('#hdId').val())
                    SendGroupMessage(data.Id, data.UserName, data.Value, true);
                break;
        }
    };

    this.onError = function (err) { alert(err); };

    this.send = function (msg) {
        var url = '/handler/?msg=' + msg + '&id=' + $('#hdId').val();
        connectTries++;

        if (connectTries > 3) {
            //self.send(window.JSON.stringify({ Type: 11, UserName: $('#hdUserName').val(), Id: $('#hdId').val() }));
        }

        if ("ActiveXObject" in window) { // IE
            createActiveXFrame(url);
        } else {
            createIframe(url);
        }

        reconnectTimer = setTimeout(function () {
            if (!self.isConnected()) {
                //self.send(window.JSON.stringify({ Type: 11, UserName: $('#hdUserName').val(), Id: $('#hdId').val() }));
            }
        }, connectTries * 2000);

        iframe.onload = function () {
            self.send(window.JSON.stringify({ Type: 11, UserName: $('#hdUserName').val(), Id: $('#hdId').val() }));
        };
    };

    this.isConnected = function () {
        return connectTries == 0; // onConnect обнуляет connectTries
    };

    function cleanIframe() {
        if (iframe) {
            iframe.src = "javascript:false";
            iframe.parentNode.removeChild(iframe); // очистка
        }
    }

    function createIframe(src) {
        cleanIframe();

        iframe = document.createElement('iframe');
        iframe.src = src || 'javascript:false';
        iframe.style.display = 'none';

        document.body.appendChild(iframe);
    }

    function createActiveXFrame(src) {
        cleanIframe();

        if (!htmlfile) {
            htmlfile = new ActiveXObject("htmlfile");
            htmlfile.open();
            htmlfile.write("<html><body></body></html>");
            htmlfile.close();
            htmlfile.parentWindow.IframeComet = self;
        }

        src = src || 'javascript:false'; // clear src
        htmlfile.body.insertAdjacentHTML('beforeEnd', "<iframe src='" + src + "'></iframe>");
        iframe = htmlfile.body.lastChild; // window in .document.parentWindow
    }

    function JoinUser(allUsers) {
        for (i = 0; i < allUsers.length; i++)
            AddUser(allUsers[i].ConnectionId, allUsers[i].UserName);
    }

    function JoimMessage(allMessage) {
        for (i = 0; i < allMessage.length; i++)
            AddMessage(allMessage[i].UserName, allMessage[i].Value);
    }

    function RemoveUser(id, name) {
        $("#" + id).remove();

        var selectobject = document.getElementById("select");
        for (var i = 0; i < selectobject.length; i++) 
            if (selectobject.options[i].value == id)
                selectobject.remove(i);
        

        var ctrId = 'private_' + id;
        $('#' + ctrId).remove();
        var disc = $('<div class="disconnect">"' + name + '" logged off.</div>');
        $(disc).hide();
        $('#divusers').prepend(disc);
        $(disc).fadeIn(200).delay(2000).fadeOut(200);
    }

    function AddUser(id, name) {
        var userId = $('#hdId').val();
        var code = "";
        var code1 = "";

        if (userId == id) {
            code = $('<div class="loginUser">' + name + "</div>");
        }
        else {
            code = $('<a id="' + id + '" class="user" >' + name + '<a>');
            code1 = $('<option value = "' + id + '">' + name + '</option>');
            $(code).dblclick(function () {

                var id = $(this).attr('id');
                if (userId != id)
                    OpenPrivateChatWindow(id, name);

            });
        }
        $("#divusers").append(code);
        $("#select").append(code1);
    }

    function AddMessage(userName, message) {
        $('#divChatWindow').append('<div class="message"><span class="userName">' + userName + '</span>: ' + message + '</div>');

        var height = $('#divChatWindow')[0].scrollHeight;
        $('#divChatWindow').scrollTop(height);
    }

    function SendGroupMessage(IwindowId, fromUserName, message, isTyping) {
        windowId = IwindowId.split(' ').join('_');

        if (!isTyping) {
            var ctrId = 'private_' + windowId;
            if ($('#' + ctrId).length == 0)
                createGroupChatWindow(windowId, ctrId, fromUserName);

            $('#' + ctrId).find('#divMessage').append('<div class="message"><span class="userName">' + fromUserName + '</span>: ' + message + '</div>');
            var height = $('#' + ctrId).find('#divMessage')[0].scrollHeight;
            $('#' + ctrId).find('#divMessage').scrollTop(height);
        }
        else if (fromUserName != $("#txtNickName").val()) {
            $('#isTypingg').html('<em>' + fromUserName + ' is typing...</em>');
            setTimeout(function () {
                $('#isTypingg').html('&nbsp;');
            }, 5000);

        }
    }

    function SendPrivateMessage(windowId, fromUserName, message, isTyping) {
        if (!isTyping) {
            var ctrId = 'private_' + windowId;
            if ($('#' + ctrId).length == 0)
                createPrivateChatWindow(windowId, ctrId, fromUserName);

            $('#' + ctrId).find('#divMessage').append('<div class="message"><span class="userName">' + fromUserName + '</span>: ' + message + '</div>');
            var height = $('#' + ctrId).find('#divMessage')[0].scrollHeight;
            $('#' + ctrId).find('#divMessage').scrollTop(height);
        }
        else {
            $('#isTypingp').html('<em>' + fromUserName + ' is typing...</em>');
            setTimeout(function () {
                $('#isTypingp').html('&nbsp;');
            }, 5000);
        }
    }

    function OpenPrivateChatWindow(id, userName) {
        var ctrId = 'private_' + id;
        if ($('#' + ctrId).length > 0) return;
        createPrivateChatWindow(id, ctrId, userName);
    }

    function createPrivateChatWindow(userId, ctrId, userName) {
        var div = '<div id="' + ctrId + '" class="ui-widget-content draggable" rel="0">' +
            '<div class="header">' +
            '<div  style="float:right;">' +
            '<img id="imgDelete" style="cursor:pointer;" src="/Content/delete.png" />' +
            '</div>' +
            '<span class="selText" rel="0">' + userName + '</span>' +
            '</div>' +
            '<div id="divMessage" class="messageArea">' +
            '</div>' +
            '<div class="buttonBar">' +
            '<label id="isTypingp" />' +
            '<input id="txtPrivateMessage" class="msgText" type="text"   />' +
            '<input id="btnSendMessage" class="submitButton button" type="button" value="Send"   />' +
            '</div>' +
            '</div>';
        var $div = $(div);

        $div.find('#imgDelete').click(function () {
            $('#' + ctrId).remove();
        });

        $div.find("#btnSendMessage").click(function () {
            $textBox = $div.find("#txtPrivateMessage");
            var msg = $textBox.val();
            if (msg.length > 0) {
                Iframe.send(window.JSON.stringify({ Type: 0, Value: msg, UserName: $('#hdUserName').val(), Id: userId + ' ' + $('#hdId').val() }));
                $textBox.val('');
            }
        });

        var chak = true;

        $div.find("#txtPrivateMessage").keypress(function (e) {
            
            if (e.which === 13) {
                $div.find("#btnSendMessage").click();
            } else if(chak ==true) {           
                Iframe.send(window.JSON.stringify({ Type: 6, UserName: $('#hdUserName').val(), Id: userId }));
                chak = false;
                setTimeout(chak = true, 2000);
            }
        });
        AddDivToContainer($div);
    }

    function createGroupChatWindow(userId, ctrId, userName) {
        var div = '<div id="' + ctrId + '" class="ui-widget-content draggable" rel="0">' +
            '<div class="header">' +
            '<div  style="float:right;">' +
            '<img id="imgDelete" style="cursor:pointer;" src="/Content/delete.png" />' +
            '</div>' +
            '<span class="selText" rel="0">' + userName + '</span>' +
            '</div>' +
            '<div id="divMessage" class="messageArea">' +
            '</div>' +
            '<div class="buttonBar">' +
            '<label id="isTypingg" />' +
            '<input id="txtGroupMessage" class="msgText" type="text"   />' +
            '<input id="btnSendMessageg" class="submitButton button" type="button" value="Send"   />' +
            '</div>' +
            '</div>';
        var $div = $(div);

        $div.find('#imgDelete').click(function () {
            $('#' + ctrId).remove();
        });

        $div.find("#btnSendMessageg").click(function () {
            $textBox = $div.find("#txtGroupMessage");
            var msg = $textBox.val();
            if (msg.length > 0) {
                Iframe.send(window.JSON.stringify({ Type: 7, Value: msg, UserName: $('#hdUserName').val(), Id: userId.split("_").join(" ") }));
                $textBox.val('');
            }
        });
        var chak1=true;

        $div.find("#txtGroupMessage").keypress(function (e) {
            if (e.which === 13) {
                $div.find("#btnSendMessageg").click();
            } else if (chak1 == true) {
                Iframe.send(window.JSON.stringify({ Type: 8, UserName: $('#hdUserName').val(), Id: userId.split("_").join(" ") }));
                chak1 = false;
                setTimeout(chak1 = true, 2000);
            }
        });
        AddDivToContainer($div);
    }

    function AddDivToContainer($div) {
        $('#divContainer').prepend($div);

        $div.draggable({
            handle: ".header",
            stop: function () {
            }
        });
    }

    function SayWhoIsTyping(name) {
        if (name != $("#txtNickName").val()) {
            $('#isTyping').html('<em>' + name + ' is typing...</em>');
            setTimeout(function () {
                $('#isTyping').html('&nbsp;');
            }, 5000);
        }
    }
};

$(function () {
    var con = window.JSON.stringify({ Type: 9, UserName: $('#hdUserName').val(), Id: $('#hdId').val() });
    Iframe.send(con);
    registerEvents();

    function registerEvents() {
        var modal = document.getElementById('myModal');

        window.onclick = function (event) {
            if (event.target == modal) {
                modal.style.display = "none";
            }
        };

        $("#btnCreategroup").click(function () {
            modal.style.display = "block";
        });

        $("#createGroup").click(function () {
            var values = $('#select').val();
            if (values.length >= 2) {
                values.push($('#hdId').val());
                var str = values.join(' ');
                Iframe.send(window.JSON.stringify({ Type: 7, UserName: $('#hdUserName').val(), Value: "Створив групу", Id: str }));
                $("#closer").click();
            }
            else {
                alert("Please select people");
            }
        });

        $("#btnSendMsg").click(function () {
            var msg = $("#txtMessage").val();
            if (msg.length > 0) {
                var userName = $('#hdUserName').val();
                var id = $('#hdId').val();
                Iframe.send(window.JSON.stringify({ Type: 1, Value: msg, Id: id ,UserName: userName}));
                $("#txtMessage").val('');
            }
        });

        var chak2=true;
        $("#txtMessage").keypress(function (e) {
            if (e.which === 13) {
                $('#btnSendMsg').click();
            }
            else if (chak2 == true) {
                Iframe.send(window.JSON.stringify({ Type: 5, UserName: $('#hdUserName').val(), Id: $('#hdId').val() }));
                chak2 = false;
                setTimeout(chak2 = true, 2000);
            }          
        });

        $("#closer").click(function () {
            modal.style.display = "none";
        });
    }  
});
