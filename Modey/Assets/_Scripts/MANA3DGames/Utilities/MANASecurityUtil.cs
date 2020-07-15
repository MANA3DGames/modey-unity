// MANAUtil: by Mahmoud A.N. Abu Obaid
using UnityEngine;
using System.IO;
using System.Security.Cryptography;

namespace MANA3DGames.Utilities.Security
{
    public static class PlayerPrefsExtensions
	{
		#region Private static functions

		private static object MisMatchKey( string key, PrefsDataType type )
		{
			object temp = null;
			
			switch ( type )
			{
			case PrefsDataType.INT:
				// Mismatch Integer Value
				temp = -1;
				break;
			case PrefsDataType.FLOAT:
				// Mismatch Float Value
				temp = -1;
				break;
			case PrefsDataType.STRING:
				// Mismatch String Value
				temp = "";
				break;
			default:
				// Ignore this one
				return null;
			}
			
			return temp;
		}

		#endregion
		

		#region public static functions

		public static void SetSecure( this PlayerPrefs obj, string key, int val )
		{
			savePlayerPref( key, System.Convert.ToString( val ) );
		}
		
		public static void SetSecure( this PlayerPrefs obj, string key, float val)
		{
			savePlayerPref( key, System.Convert.ToString( val ) );
		}
		
		public static void SetSecure( this PlayerPrefs obj, string key, string val)
		{
			savePlayerPref( key, val );
		}
		
		public static string GetSecureString( this PlayerPrefs obj, string key )
		{
			if ( !checkHasKey( key ) ) return "";
			return ( playerPrefDishutter201( key ) ) ? getCore( re201_turDCode ) : ( string )MisMatchKey( key, PrefsDataType.STRING ); 
		}
		
		public static float GetSecureFloat( this PlayerPrefs obj, string key )
		{
			if ( !checkHasKey( key ) ) return 0;
			return ( playerPrefDishutter201( key ) ) ? ( float )System.Convert.ToDouble( getCore(re201_turDCode) ) : ( float )MisMatchKey( key, PrefsDataType.FLOAT );
		}
		
		public static int GetSecureInt( this PlayerPrefs obj, string key )
		{
			if ( !checkHasKey( key ) ) return 0;
			return ( playerPrefDishutter201( key ) ) ? System.Convert.ToInt32( System.Convert.ToDouble( getCore(re201_turDCode) ) ) : ( int )MisMatchKey( key, PrefsDataType.INT );
		}
		
		public static bool HasSecureKey( this PlayerPrefs obj, string key )
		{
			return checkHasKey( key );
		}


		#endregion


		#region private static funtions

		private enum PrefsDataType { INT, FLOAT, STRING };
		
		
		private static byte[] stringToBytes( string str )
		{
			byte[] data = new byte[str.Length * 2];
			
			for ( int i = 0; i < str.Length; ++i )
			{
				char ch = str[i];
				data[i * 2] = ( byte )( ch & 0xFF );
				data[i * 2 + 1] = ( byte )( ( ch & 0xFF00 ) >> 8 );
			}
			
			return data;
		}
		
		private static string stringFromBytes( byte[] arr )
		{
			char[] ch = new char[arr.Length / 2];
			
			for ( int i = 0; i < ch.Length; ++i )
			{
				ch[i] = ( char )( ( int )arr[i * 2] + ( ( ( int )arr[i * 2 + 1] ) << 8 ) );
			}
			return new string( ch );
		}
		
		private static bool checkHasKey( string key )
		{
			return ( PlayerPrefs.HasKey( System.Convert.ToBase64String( strCode201ToByteCode032( key, Crypto.Key, Crypto.IV ) ) ) ) ? true : false;
		}
		
		private static bool playerPrefDishutter201( string res201key )
		{
			re201_turDCode = byteCode301ToStrCode201( System.Convert.FromBase64String( PlayerPrefs.GetString( System.Convert.ToBase64String( strCode201ToByteCode032( res201key, Crypto.Key, Crypto.IV ) ) ) ), Crypto.Key, Crypto.IV );
			
			if ( md5Sum( re201_turDCode ) == PlayerPrefs.GetString( System.Convert.ToBase64String( strCode201ToByteCode032( res201key + addCode_19RPart, Crypto.Key, Crypto.IV ) ) ) ) return true; else return false;
		}
		
		private static RijndaelManaged Crypto;
		private static string pefEnc_Code_For201Ruff;
		private static string decEnc_Code_M391M;
		private static string addCode_19RPart;
		private static string re201_turDCode;
		private static bool initCode_101;



		// For testing.
		public static void SetKeys_test()
		{
			string[] keys = { "qweqweqwejkckcirnvyt97234h239ewfiuwf", 
				              "nch36riv943hc6o023n87tv34j89v90jmch34", 
				              "m74bc660megcg6fbyubv53445niocwo22", 
				              "nvq2659bn75bnvgdivdspmfue64bcasmnrkvhf232fwd",  
				              "nvryb85hj7hucur9khcg36skqhte53ooijwu49j8" };
			
			PlayerPrefs.SetString( "strD301CodeCoreSCodeK", keys[0] );
			PlayerPrefs.SetString( "strD301CodeCoreV2CodeI", keys[1] );
			PlayerPrefs.SetString( "strPefEnc_Code_For201Ruff", keys[2] );
			PlayerPrefs.SetString( "strDecEnc_Code_M391M", keys[3] );
			PlayerPrefs.SetString( "strAddCode_19RPart", keys[4] );
		}

		public static void SetKeys( string[] coreCode )
		{
			PlayerPrefs.SetString( "strD301CodeCoreSCodeK", coreCode[0] );
			PlayerPrefs.SetString( "strD301CodeCoreV2CodeI", coreCode[1] );
			PlayerPrefs.SetString( "strPefEnc_Code_For201Ruff", coreCode[2] );
			PlayerPrefs.SetString( "strDecEnc_Code_M391M", coreCode[3] );
			PlayerPrefs.SetString( "strAddCode_19RPart", coreCode[4] );
		}

		public static void Initialize( this PlayerPrefs obj )
		{
			if ( initCode_101 )
				return;
			
			string strD301CodeCoreSCodeK = PlayerPrefs.GetString( "strD301CodeCoreSCodeK" );
			string strD301CodeCoreV2CodeI = PlayerPrefs.GetString( "strD301CodeCoreV2CodeI" );
			string strPefEnc_Code_For201Ruff = PlayerPrefs.GetString( "strPefEnc_Code_For201Ruff" );
			string strDecEnc_Code_M391M = PlayerPrefs.GetString( "strDecEnc_Code_M391M" );
			string strAddCode_19RPart = PlayerPrefs.GetString( "strAddCode_19RPart" );
			
			Crypto = new RijndaelManaged();
			
			byte[] d301CodeCore = new byte[16];
			for ( int i = 0; i < d301CodeCore.Length; i++ )
				d301CodeCore[i] = ( byte )System.Convert.ToInt32( System.Convert.ToChar( strD301CodeCoreSCodeK.Substring( i + ( i^2 ) , 1 ) ) );
			Crypto.Key = d301CodeCore;
			for ( int i = 0; i < d301CodeCore.Length; i++ )
				d301CodeCore[i] = ( byte )System.Convert.ToInt32( System.Convert.ToChar( strD301CodeCoreV2CodeI.Substring( i + ( i^2 ), 1 ) ) );
			Crypto.IV = d301CodeCore;
			
			pefEnc_Code_For201Ruff = strPefEnc_Code_For201Ruff;
			decEnc_Code_M391M = strDecEnc_Code_M391M;
			addCode_19RPart = strAddCode_19RPart;
			
			initCode_101 = true;
			
			PlayerPrefs.DeleteKey( "strD301CodeCoreSCodeK" );
			PlayerPrefs.DeleteKey( "strD301CodeCoreV2CodeI" );
			PlayerPrefs.DeleteKey( "strPefEnc_Code_For201Ruff" );
			PlayerPrefs.DeleteKey( "strDecEnc_Code_M391M" );
			PlayerPrefs.DeleteKey( "strAddCode_19RPart" );
		}
		
		private static void savePlayerPref( string rs201Key, string re201Val )
		{
			PlayerPrefs.SetString( System.Convert.ToBase64String( strCode201ToByteCode032( rs201Key, Crypto.Key, Crypto.IV ) ), System.Convert.ToBase64String( strCode201ToByteCode032( pefEnc_Code_For201Ruff + re201Val + decEnc_Code_M391M, Crypto.Key, Crypto.IV ) ) );
			
			PlayerPrefs.SetString( System.Convert.ToBase64String( strCode201ToByteCode032( rs201Key + addCode_19RPart, Crypto.Key, Crypto.IV ) ), md5Sum( pefEnc_Code_For201Ruff + re201Val + decEnc_Code_M391M ) );
		}
		
		private static string getCore( string str202 )
		{
			string core202 = str202.Replace( decEnc_Code_M391M, "" );
			core202 = core202.Replace( pefEnc_Code_For201Ruff, "" );
			return core202;
		}
		
		private static string md5Sum( string str15Part1 )
		{
			System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
			byte[] bytes = ue.GetBytes( str15Part1 );
			System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
			byte[] hashBytes = md5.ComputeHash( bytes );
			string hashString = "";
			for ( int i = 0; i < hashBytes.Length; i++ ) { hashString += System.Convert.ToString( hashBytes[i], 16 ).PadLeft( 2, '0' ); }
			return hashString.PadLeft( 32, '0' );
		}
		
		private static byte[] strCode201ToByteCode032( string strPrefCode_201, byte[] Key, byte[] IV )
		{
			RijndaelManaged Crypto = null;
			MemoryStream  MemStream = null;
			ICryptoTransform Encryptor = null;
			CryptoStream Crypto_Stream = null;
			System.Text.UTF8Encoding Byte_Transform = new System.Text.UTF8Encoding();
			byte[] PlainBytes = Byte_Transform.GetBytes( strPrefCode_201 );
			
			try
			{
				Crypto = new RijndaelManaged();
				Crypto.Key = Key;
				Crypto.IV = IV;
				MemStream = new MemoryStream();
				Encryptor = Crypto.CreateEncryptor( Crypto.Key, Crypto.IV );
				Crypto_Stream = new CryptoStream( MemStream, Encryptor, CryptoStreamMode.Write );
				Crypto_Stream.Write( PlainBytes, 0, PlainBytes.Length );
				
			}
			finally
			{
				if ( Crypto != null )
					Crypto.Clear();
				Crypto_Stream.Close();
			}
			
			return MemStream.ToArray();
		}
		
		private static string byteCode301ToStrCode201( byte[] byteCode102, byte[] Key, byte[] IV )
		{
			RijndaelManaged Crypto = null;
			MemoryStream MemStream = null;
			ICryptoTransform Decryptor = null;
			CryptoStream Crypto_Stream = null;
			StreamReader Stream_Read = null;
			string strLocal_Codebbyte = "";
			
			try
			{
				Crypto = new RijndaelManaged();
				Crypto.Key = Key;
				Crypto.IV = IV;
				MemStream = new MemoryStream( byteCode102 );
				Decryptor = Crypto.CreateDecryptor( Crypto.Key, Crypto.IV );
				Crypto_Stream = new CryptoStream( MemStream, Decryptor, CryptoStreamMode.Read );
				Stream_Read = new StreamReader( Crypto_Stream );
				strLocal_Codebbyte = Stream_Read.ReadToEnd();
			}
			catch { Debug.Log( "Reading Error!" ); }
			finally
			{
				if ( Crypto != null )
					Crypto.Clear();
				
				MemStream.Flush();
				MemStream.Close();
			}
			
			return strLocal_Codebbyte;
		}

		#endregion

	}
}
