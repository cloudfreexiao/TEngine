﻿using System;
using Cysharp.Threading.Tasks;
using Launcher;
using TEngine;
using YooAsset;
using ProcedureOwner = TEngine.IFsm<TEngine.IProcedureModule>;

namespace Procedure
{
    /// <summary>
    /// 流程 => 用户尝试更新清单
    /// </summary>
    public class ProcedureUpdateManifest: ProcedureBase
    {
        public override bool UseNativeDialog { get; }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            Log.Info("更新资源清单！！！");
            
            LauncherMgr.Show(UIDefine.UILoadUpdate,$"更新清单文件...");
            
            UpdateManifest(procedureOwner).Forget();
        }

        private async UniTaskVoid UpdateManifest(ProcedureOwner procedureOwner)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            
            var operation = _resourceModule.UpdatePackageManifestAsync(_resourceModule.PackageVersion);
            
            await operation.ToUniTask();
            
            if(operation.Status == EOperationStatus.Succeed)
            {
                //更新成功
                //注意：保存资源版本号作为下次默认启动的版本!
                
                if (_resourceModule.PlayMode == EPlayMode.WebPlayMode ||
                    _resourceModule.UpdatableWhilePlaying)
                {
                    // 边玩边下载还可以拓展首包支持。
                    ChangeState<ProcedurePreload>(procedureOwner);
                    return;
                }
                ChangeState<ProcedureCreateDownloader>(procedureOwner);
            }
            else
            {
                Log.Error(operation.Error);
                
                LauncherMgr.ShowMessageBox($"用户尝试更新清单失败！点击确认重试 \n \n <color=#FF0000>原因{operation.Error}</color>", MessageShowType.TwoButton,
                    LoadStyle.StyleEnum.Style_Retry
                    , () => { ChangeState<ProcedureUpdateManifest>(procedureOwner); }, UnityEngine.Application.Quit);
            }
        }
    }
}