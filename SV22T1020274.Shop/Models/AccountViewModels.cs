using System.ComponentModel.DataAnnotations;

namespace SV22T1020274.Shop.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp")]
    [Display(Name = "Xác nhận mật khẩu")]
    public string ConfirmPassword { get; set; } = "";
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = "";

    [Display(Name = "Ghi nhớ đăng nhập")]
    public bool RememberMe { get; set; }
}

public class ProfileEditViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên khách hàng")]
    [Display(Name = "Tên khách hàng")]
    public string CustomerName { get; set; } = "";

    [Display(Name = "Tên giao dịch")]
    public string ContactName { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng chọn tỉnh/thành")]
    [Display(Name = "Tỉnh/thành")]
    public string? Province { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
    [Display(Name = "Địa chỉ")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập điện thoại")]
    [Display(Name = "Điện thoại")]
    public string? Phone { get; set; }

    [Display(Name = "Email")]
    public string Email { get; set; } = "";
}

public class CustomerChangePasswordViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu hiện tại")]
    public string OldPassword { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string NewPassword { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Xác nhận không khớp")]
    [Display(Name = "Xác nhận mật khẩu mới")]
    public string ConfirmPassword { get; set; } = "";
}

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn tỉnh/thành giao hàng")]
    [Display(Name = "Tỉnh/thành")]
    public string? DeliveryProvince { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
    [Display(Name = "Địa chỉ giao hàng")]
    public string? DeliveryAddress { get; set; }

    [Display(Name = "Số điện thoại nhận hàng")]
    public string? DeliveryPhone { get; set; }
}
