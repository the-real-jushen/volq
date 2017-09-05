(function () {
    QUnit.config.testTimeout = 10000;

    var stringformat = QUnit.stringformat;

    module('Web API Get');

    //Web API GET模块需要测试的web api url
    var apiUrls = [
        '/api/user/all',
        '/api/user/email?email=1@qq.com',
        '/api/organizer/be_in',
        '/api/organizer/to_join',
        '/api/organizer/appliedorganization',
        '/api/organizer/activitytocheckin',
        '/api/organizer/activitytocheckout'
    ];
    var apiUrlslen = apiUrls.length;
    
    // Test only that the Web API responded to the request with 'success'
    var reachTest = function (url) {
        $.ajax({
            url: url,
            dataType: 'json',
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
            success: function (result) {
                ok(true, 'GET succeeded for ' + url);
                ok(true, 'GET retrieved some data:' + JSON.stringify(result));
                start();
            },
            error: function (result) {
                ok(false,
                    stringformat('GET on \'{0}\' failed with status=\'{1}\': {2}',
                        url, result.status, JSON.stringify(result)));
                start();
            }
        });
    };
    
    // Returns an endpointTest function for a given URL
    var getTestGenerator = function (url) {
        return function () { reachTest(url); };
    };

    // Test each endpoint in apiUrls
    for (var i = 0; i < apiUrlslen; i++) {
        var apiUrl = apiUrls[i];
        asyncTest(
            'API can be reached: ' + apiUrl,
            getTestGenerator(apiUrl));
    };

})();