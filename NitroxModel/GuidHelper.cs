using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Helper
{
    public class NitroxIdentifier : MonoBehaviour
    {
        private static readonly Dictionary<string, NitroxIdentifier> identifiers = new Dictionary<string, NitroxIdentifier>();
        private string id;
        [HideInInspector] [SerializeField] private string classId;

        public NitroxIdentifier()
        {
            id = GuidHelper.GetIdForEntity(gameObject.transform.position.ToString() + gameObject.transform.rotation.ToString());
        }
        
        
        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && id != value)
                {
                   // Log.InGame("updating id "+ id+" to " + value);
                    Unregister();
                    id = value;
                    Register();
                }
            }
        }

        private void Awake()
        {
            //Log.InGame("Starting object with guid "+id);
            Register();
        }

        private void OnDestroy()
        {
           // Log.InGame("Destroying object with guid "+id);
            Unregister();
        }

        private void Register()
        {
            string id = this.id;
            if (string.IsNullOrEmpty(id))
                return;
            NitroxIdentifier uniqueIdentifier;
            if (identifiers.TryGetValue(id, out uniqueIdentifier))
            {
                if (uniqueIdentifier == this)
                    return;
                identifiers[id] = this;
            }
            else
                identifiers.Add(id, this);
        }

        private void Unregister()
        {
            string id = this.id;
            if (string.IsNullOrEmpty(id))
                return;
            NitroxIdentifier uniqueIdentifier;
            if (identifiers.TryGetValue(id, out uniqueIdentifier))
            {
                if (!(uniqueIdentifier == this))
                    return;
                identifiers.Remove(id);
            }
            else
                Log.InGame("Unregistering unique identifier '" + id + "' failed because it is not registered.");
        }

        public string EnsureGuid(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                var newId = Guid.NewGuid().ToString();
                Log.InGame("ERROR: guid is empty, current "+id+ " new: " + newId);
                return newId;
            }

            return guid;
        }

        public static bool TryGetIdentifier(string id, out NitroxIdentifier uid)
        {
            if (!string.IsNullOrEmpty(id))
                return identifiers.TryGetValue(id, out uid);
            uid = null;
            return false;
        }

        public static IEnumerable<NitroxIdentifier> AllIdentifiers
        {
            get { return identifiers.Values; }
        }

        public static IDictionary<string, NitroxIdentifier> DebugIdentifiers()
        {
            return identifiers;
        }
    }

    public static class GuidHelper
    {
        public static string GetIdForEntity(string input)
        {
            return GetHashString(input);
        }
        
        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();  //or use SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }
        
        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        
        public static GameObject RequireObjectFrom(string guid)
        {
            Optional<GameObject> gameObject = GetObjectFrom(guid);
            Validate.IsPresent(gameObject, "Game object required from guid: " + guid);
            return gameObject.Get();
        }

        // Feature parity of UniqueIdentifierHelper.GetByName() except does not do the verbose logging
        public static Optional<GameObject> GetObjectFrom(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return Optional<GameObject>.Empty();
            }

            NitroxIdentifier uniqueIdentifier;

            if (!NitroxIdentifier.TryGetIdentifier(guid, out uniqueIdentifier))
            {
                return Optional<GameObject>.Empty();
            }

            if (uniqueIdentifier == null)
            {
                return Optional<GameObject>.Empty();
            }

            return Optional<GameObject>.Of(uniqueIdentifier.gameObject);
        }

        public static string GetGuid(this GameObject gameObject)
        {
            return GetUniqueIdentifier(gameObject).Id;
        }

        public static void SetNewGuid(this GameObject gameObject, string guid)
        {
            GetUniqueIdentifier(gameObject).Id = guid;
        }

        private static NitroxIdentifier GetUniqueIdentifier(GameObject gameObject)
        {
           NitroxIdentifier uniqueIdentifier = gameObject.GetComponent<NitroxIdentifier>();

            if (uniqueIdentifier == null)
            {
                uniqueIdentifier = gameObject.AddComponent<NitroxIdentifier>();
              /*  var constructable = gameObject.GetComponent<Constructable>();
                TechType techtype = TechType.None;
                if (constructable != null)
                {
                    techtype = constructable.techType;
                }
                Log.InGame("Created new id " + uniqueIdentifier.Id + " " +techtype);*/
            }

            return uniqueIdentifier;
        }
    }
}
