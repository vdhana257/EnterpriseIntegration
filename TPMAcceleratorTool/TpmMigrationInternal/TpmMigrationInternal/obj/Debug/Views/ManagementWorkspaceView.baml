   M S B A M L   `   `   `  ����G  CTpmMigration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null�Mclr-namespace:Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration?Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration  5      ]    WMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.ManagementWorkspaceView   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ftpmMigrationMclr-namespace:Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration     5        � 	Resources  5
      _   YMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.BoolToVisibilityConverter   boolToVisibilityConverterl  fMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.StatusInfoTypeEnumToImagePathConverter * &statusInfoTypeEnumToImagePathConverterk  eMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.ProgressBarStateToVisibilityConverter ) %progressBarStateToVisibilityConverter��%W   &	        &	      &	 &       5   
   61     5   
   6>     5   
   6=   5      .��5      � -   5        $��Layout��5       � RowDefinitions 5   
   �� 5      $��33 �6   �� 5      $��* �6   �� 5      $
��Auto �6   �� 5      $��40 �6   .+�5   
   � $��0��6   $��Stretch=�6   $��Stretch=�6:     DarkBlueBrush#��[ 6V   $��
-1 -1 -1 0q�6�    � ColumnDefinitions 5      �� 5      $��10 �6#   �� 5      $
��Auto �6#   �� 5      $��10 �6#   �� 5      $��* �6#   �� 5      $��25 �6#   �� 5       $��25 �6#   .+�5"      �� $��1��6   $��Center=�6$   $��23��6?   $��Uniform=�6K   $2��,/TpmMigration;component/Images/azurelogo.png��6]   �� 5#      -   5        $��3��5#      $��Segoe UI)�6r   $��18(�6�   $��Center=�6�   
���������6(   �� *TpmMigrationToolTitle+  	Resources �� Source# [ 6�   �� 5$      -   5         ��Name$ buttonMinimize��5$   %   $��4��6   $��Center=�6=   $��0 0 0 3q�6X   $��20��6i     MinimizeButton#��[ 6t   6�   �� 5%      -   5        $ buttonCancel��5%   %   $��5��6   $��Center=�6;   $��20��6V     CancelButton#��[ 6a   6�   5&      � 5)   
   $��1��6    5*      �� 5+      $	��170 �6#   �� 5,      $��20 �6#   �� 5-      $��* �6#   .+�51      Y   SMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.WizardStepsListView  -   5          Name$ WizardStepsList��51   ?   $��0��6/    � DataContext 6X   �� *WizardNavigationViewModel+�� 54      $��2��6   $��Stretch=�6+   $��Stretch=�6G   $
��Auto=�6e   .��55      �  56      �� 57      $��* �6+   �� 58      $��20 �6+    5:      �� 5;      $��10 �6(   �� 5<      $��40 �6(   �� 5=      $��20 �6(   �� 5>      $��* �6(   �� 5?      $��20 �6(   .+�5A      �� -   5         ��Name$ Title��5A       $��1��6/   $��0��6<   $
��Left=�6L   $��Center=�6g   $��Normal$�6�   $��14(�6�   
���   �$��	ExtraBold"�6�    � 5B      -   5          �Name$ WizardContent��5B   9   $��3��6   $��0��6)   $
��Left=�6P   $	��Top=�6k   5C      5D      5E      �� 5H   
   -   5        	 ��Name$	 	StatusBar��5H      $��2��6$   ���#2C2C2C$��40��6F   $��70��6U    6d   �� *StatusBarViewModel+y�5I      �� *	IsVisible+
 �� 	Converter#
 [  ._�5J      �  5K      �� 5L      $��10 �6'   64   �� 5M      $
��Auto �6'   66   �� 5N      $��10 �6'   64   �� 5O      $��* �6'   63   �� 5P      $
��Auto �6'   66   �� 5Q      $��5 �6'   63   .+�5S      �� $��1��6   $��15��6�   $��15��6�   ��6(   �� *StatusInfoType+#
 [ 6�   �� 5T      $��3��6    �� 
IsReadOnly . $	��750��6<   
�������
������ 
������ $��0q�6�   $
��Left=�6�    �� TextWrapping$
 Wrap=�6�   " �� VerticalScrollBarVisibility$
 Auto=�5U      $��Center=�6A   ��5T   �   �� *StatusBarText+5U   A   �� 5W        OKButton#��[ 6   $��0 0 0 0q�6;   $��70��6L   $��90��6X   $��4��6c   ��6s   �� *StatusBarClearCommand+5X      5Y      ]� 5`   
   -	   5         ]�Name$ WizardNavigator��5`   !   $��3��6   #��[ 6l    6:   �� *WizardNavigationViewModel+.+�5a      ��   CommandMenuButtonStyle#��[ 6m   $��Right=�6�   y�6   �� *
IsLastStep+#
 [  ��6�   �� *CloseCommand.DisplayName+��6�   �� *CloseCommand+6�   �� 5b      #��[ 6�   $��Right=�6  y�6   �� *
IsLastStep+#
 [   �� ConverterParameter$ reverse����6�   �� *MoveNextCommand.DisplayName+��6�   �� *MoveNextCommand+6  �� 5c      #��[ 6�   $��Right=�6  y�6   �� *IsPreviousVisible+#
 [  $ reverse����6�   �� *!MovePreviousCommand.DisplayName+��6�   �� *MovePreviousCommand+6  �� 5d      6   �� 5e      $
��Left=�67   $��Center=�6R   
���������6   �� *ContactSupport+6�   5f      �� 5i   
   
������ $��4��6'   y�5j      �� *'%ProgressBarViewModel.ProgressBarState+#
 [ � 5k   
    J� Opacity$ 1Q�6   $
��Auto��6   $��1��6)   $��	170 0 0 0q�66   $��Bottom=�6I   $��Stretch=�6d    5l      �� *ProgressBarViewModel+ 5m      �� 5n      $
��Auto �6    �� 5o      $��10 �6    �� 5p      $��15 �6    .+�5r      �� $��0��6   $��Medium"�6%   $��Center=�6�   
���   �$
��Wrap=�6�   y�6:   �� *ProgressBarState+#
 [ ��6�   �� *ProgressBarText+6�   � 5s      -
   5         �Name$ ProgressBar��5s   �    	 DefaultProgressBarStyle#��[	 6   $��2��6K   $	��Top=�6�   $��0q�6�   $��15��6�    � IsIndeterminate .  J� RenderTransformOrigin$ 0.5,0.5&�6  y�6X   �� *ProgressBarState+#
 [ z�5t      V� 5u      .~�5v      �� �� 5w       �� AngleX$ 3.366Q�6(   �� 5x      T� 5y       T� X$ 0.441Q�6-   5z      5|      5}      5~      5      