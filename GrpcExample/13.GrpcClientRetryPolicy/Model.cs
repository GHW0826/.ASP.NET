using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcClient;


public class AuthUser
{
    public string UserId { get; set; } = "";
    public string Role { get; set; } = "User";
}

public class LoginResponse
{
    public string token { get; set; } = "";
};
