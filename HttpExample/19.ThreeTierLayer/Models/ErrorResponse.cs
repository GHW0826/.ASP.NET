﻿namespace ThreeTierLayer.Models;

public class ErrorResponse
{
    public string Message { get; set; }
    public string Code { get; set; }  // Optional: "Unauthorized", "Forbidden", etc.
}
