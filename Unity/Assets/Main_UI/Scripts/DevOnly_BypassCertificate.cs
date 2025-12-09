using UnityEngine.Networking;

public class DevOnly_BypassCertificate : CertificateHandler
{
    // 공용 인증서 무시 핸들러 (로컬 개발용)
    protected override bool ValidateCertificate(byte[] certificateData) { return true; }
}
