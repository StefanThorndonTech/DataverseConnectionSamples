//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MsalServerToServerEarlyBoundSample.Core.Dataverse
{
	
	[System.Runtime.Serialization.DataContractAttribute()]
	public enum SystemUser_InviteStatusCode
	{
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		[OptionSetMetadataAttribute("Invitation Accepted", 4)]
		InvitationAccepted = 4,
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		[OptionSetMetadataAttribute("Invitation Expired", 3)]
		InvitationExpired = 3,
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		[OptionSetMetadataAttribute("Invitation Near Expired", 2)]
		InvitationNearExpired = 2,
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		[OptionSetMetadataAttribute("Invitation Not Sent", 0)]
		InvitationNotSent = 0,
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		[OptionSetMetadataAttribute("Invitation Rejected", 5)]
		InvitationRejected = 5,
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		[OptionSetMetadataAttribute("Invitation Revoked", 6)]
		InvitationRevoked = 6,
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		[OptionSetMetadataAttribute("Invited", 1)]
		Invited = 1,
	}
}