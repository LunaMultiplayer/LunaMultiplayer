using System;
using System.Collections.Generic;
using UnityEngine;

namespace LunaClient.ModuleStore
{
    class TestMod:PartModule
    {
        public Transform structTransform;

        [KSPField]
        public string attachNodeNames = "bottom";

        [KSPField]
        public float nodeRadius = 0.4f;

        [KSPField]
        public string rootObject = "Fairing";

        [KSPField]
        public bool spawnManually;

        [KSPField(isPersistant = true)]
        public bool spawnState;

        [KSPField]
        public bool reverseVisibility;

        [KSPField(isPersistant = true)]
        public bool visibilityState = true;

        private void CheckDisplay()
        {
            string[] array = this.attachNodeNames.Split(new char[]
            {
                ','
            });
            List<AttachNode> list = new List<AttachNode>();
            foreach (string nodeId in array)
            {
                AttachNode attachNode = base.part.FindAttachNode(nodeId);
                if (attachNode != null)
                {
                    for (; ; )
                    {
                        switch (2)
                        {
                            case 0:
                                continue;
                        }
                        break;
                    }
                    if (!true)
                    {
                        //RuntimeMethodHandle runtimeMethodHandle = methodof(ModuleStructuralNode.CheckDisplay()).MethodHandle;
                    }
                    list.Add(attachNode);
                }
            }
            for (; ; )
            {
                switch (3)
                {
                    case 0:
                        continue;
                }
                break;
            }
            bool flag = false;
            if (this.visibilityState)
            {
                for (; ; )
                {
                    switch (4)
                    {
                        case 0:
                            continue;
                    }
                    break;
                }
                if (this.spawnManually)
                {
                    for (; ; )
                    {
                        switch (3)
                        {
                            case 0:
                                continue;
                        }
                        break;
                    }
                    flag = this.spawnState;
                }
                else
                {
                    using (List<AttachNode>.Enumerator enumerator = list.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            AttachNode attachNode2 = enumerator.Current;
                            if (attachNode2.attachedPart != null)
                            {
                                for (; ; )
                                {
                                    switch (2)
                                    {
                                        case 0:
                                            continue;
                                    }
                                    break;
                                }
                                flag = true;
                                goto IL_103;
                            }
                        }
                        for (; ; )
                        {
                            switch (1)
                            {
                                case 0:
                                    continue;
                            }
                            break;
                        }
                    }
                }
                IL_103:
                if (this.reverseVisibility)
                {
                    for (; ; )
                    {
                        switch (6)
                        {
                            case 0:
                                continue;
                        }
                        break;
                    }
                    flag = !flag;
                }
            }
            this.structTransform.gameObject.SetActive(flag);
        }
    }
}
