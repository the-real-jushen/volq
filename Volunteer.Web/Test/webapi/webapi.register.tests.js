(function () {
    QUnit.config.testTimeout = 10000;
    
    var okAsync = QUnit.okAsync,
        stringformat = QUnit.stringformat;
    
    var baseUrl = '',
        getMsgPrefix = function(email, rqstUrl) {
            return stringformat(
                ' of user with email=\'{0}\' to \'{1}\'',
                email, rqstUrl);
        },
        onCallSuccess = function(msgPrefix) {
            ok(true, msgPrefix + " succeeded.");
        },
        onError = function (result, msgPrefix) {
            ok(false, msgPrefix + stringformat(' failed with status=\'{0}\': {1}.',result.status, result.responseText));
        };
        var testUrl,
        testMsgBase,
        testUser,
        testEmail;

    module('Web API Person update tests');
       
    test('Can register the test User',
        function () {
            testUser = { Email: "12345@qq.com", Password: "123456", Name: "lq", Role: "b" }
            testUrl = stringformat('/api/user/email?email={0}', testUser.Email);
            testMsgBase = getMsgPrefix(testUser.Email, testUrl);
            stop();
            getTestUser(registerTestUser);
        }
    );

    // Step 1: Get test user (this fnc is re-used several times)
    function getTestUser(succeed) {
        var msgPrefix = 'GET' + testMsgBase;
        $.ajax({
            type: 'GET',
            url: testUrl,
            async:true,
            xhrFields: {
                withCredentials: true
            },
            crossDomain: true,
            beforeSend: function (request) {
                request.setRequestHeader('Token', '8e1bc115-d64e-4c2a-98df-104edd90e672');
            },
            headers: {
                'Access-Control-Allow-Credentials': 'true',
            },
            success: function(result) {
                onCallSuccess(msgPrefix);
                if (result != null)
                {
                    okAsync(result.Email === testUser.Email, stringformat("returned key [{0}] matches testUser Email.", testUser.Email));
                }
                if (typeof succeed !== 'function') {
                    start();
                    return;
                } else {
                    succeed(result);
                    start();
                };
            },
            error: function (result) {
                onError(result, msgPrefix);
            }
        });
    };

    // Step 2: Change test person and save it
    function registerTestUser() {
        //testUser = user;

        var msgPrefix = 'POST (register)' + testMsgBase,
            dataUser = JSON.stringify(testUser);

        $.ajax({
            type: 'POST',
            url: '/api/user/register/',
            data: dataUser,
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            success: function() {
                onCallSuccess(msgPrefix);
                getTestUser(confirmUpdated);
            },
            error: function (result) {
                onError(result, msgPrefix);
            }
        });
        start();
    };

    // Step 3: Confirm test person updated, then call restore
    function confirmUpdated(user) {
        okAsync(user.Email === testUser.Email, "test user was registered ");
    };
})();