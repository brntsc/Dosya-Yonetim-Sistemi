// Global değişkenler
var FormatDate = function(d) {
    if (!d) return '-';
    
    try {
        // API'den gelen tarih formatını düzelt
        var dateStr = d.replace('T', ' ').split('.')[0];
        var date = new Date(dateStr);
        
        if (isNaN(date.getTime())) {
            console.error('Geçersiz tarih:', d);
            return '-';
        }
        
        var day = date.getDate().toString().padStart(2, '0');
        var month = (date.getMonth() + 1).toString().padStart(2, '0');
        var year = date.getFullYear();
        var hour = date.getHours().toString().padStart(2, '0');
        var minute = date.getMinutes().toString().padStart(2, '0');
        
        return `${day}.${month}.${year} ${hour}:${minute}`;
    } catch (e) {
        console.error('Tarih formatlama hatası:', e, 'Gelen tarih:', d);
        return '-';
    }
};

var parseJwt = function(token) {
    var base64Url = token.split('.')[1];
    var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    var jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));

    return JSON.parse(jsonPayload);
};

// Sayfa yüklendiğinde çalışacak kodlar
$(document).ready(function() {
    var token = localStorage.getItem("token");
var userRoles = [];
    
if (token == null) {
    $(".NotLogined").show();
    $(".Logined").hide();
} else {
    $(".NotLogined").hide();
    $(".Logined").show();
    var payload = parseJwt(token);
    var username = payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"];
    userRoles = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
    var userPhoto = payload["UserPhoto"];
    $("#ImgUserPhoto").attr("src", "https://localhost:7218/Files/UserPhotos/" + userPhoto);
    $("#UserName").html(username);
    
    var userId = payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
    var roleName = "";
    if (userRoles.includes("Admin")) {
        roleName = "Sistem Yönetici";
    } else {
            roleName = "Normal Üye";
    }
    $("#divRole").html(roleName);
    }

    $("#Logout").click(function() {
    logout();
    });
});

// Token yenileme fonksiyonu
function refreshToken() {
    var token = localStorage.getItem("token");
    if (!token) return;

    $.ajax({
        url: '@ViewBag.ApiBaseURL/User/RefreshToken',
        type: 'POST',
        headers: {
            'Authorization': 'Bearer ' + token
        },
        success: function(response) {
            if (response.Status) {
                localStorage.setItem("token", response.Message);
            } else {
                console.error('Token yenileme başarısız:', response.Message);
                localStorage.removeItem("token");
                window.location.href = "/Admin/Login";
            }
        },
        error: function(xhr) {
            console.error('Token yenileme hatası:', xhr);
            localStorage.removeItem("token");
            window.location.href = "/Admin/Login";
        }
    });
}

// Her 4 dakikada bir token'ı yenile
setInterval(refreshToken, 4 * 60 * 1000);

function logout() {
    localStorage.removeItem("token");
    window.location.href = "/";
}


